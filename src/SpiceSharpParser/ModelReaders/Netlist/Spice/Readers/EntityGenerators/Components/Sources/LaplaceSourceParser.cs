using System;
using System.Linq;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using ExpressionParser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceSourceParser
    {
        private const string InputErrorMessage = "laplace input expression must be V(node) or V(node1,node2)";

        public bool IsLaplaceSource(ParameterCollection parameters)
        {
            return parameters != null
                && parameters.Count >= 3
                && parameters[2] is WordParameter wordParameter
                && string.Equals(wordParameter.Value, "laplace", StringComparison.OrdinalIgnoreCase);
        }

        public LaplaceSourceDefinition ParseVoltageControlledSource(
            string sourceName,
            ParameterCollection parameters,
            IReadingContext context)
        {
            if (!IsLaplaceSource(parameters))
            {
                return null;
            }

            if (!HasOutputNodes(parameters, context))
            {
                return null;
            }

            if (parameters.Count == 3)
            {
                AddError(context, "laplace expects input expression", parameters[2].LineInfo);
                return null;
            }

            if (parameters.Count > 4)
            {
                AddError(context, GetUnsupportedExtraParameterMessage(parameters.Skip(4)), parameters[3].LineInfo);
                return null;
            }

            var assignment = parameters[3] as ExpressionAssignmentParameter;
            if (assignment == null)
            {
                AddError(context, "laplace expects input and transfer expressions separated by '='", parameters[3].LineInfo);
                return null;
            }

            if (string.IsNullOrWhiteSpace(assignment.LeftExpression))
            {
                AddError(context, "laplace expects input expression", assignment.LineInfo);
                return null;
            }

            if (string.IsNullOrWhiteSpace(assignment.RightExpression))
            {
                AddError(context, "laplace expects transfer expression", assignment.LineInfo);
                return null;
            }

            if (!TryParseVoltageInput(assignment.LeftExpression, context, out var input))
            {
                AddError(context, InputErrorMessage, assignment.LineInfo);
                return null;
            }

            LaplaceTransferFunction transferFunction;
            try
            {
                transferFunction = new LaplaceExpressionParser(
                    context.EvaluationContext,
                    lineInfo: assignment.LineInfo).Parse(assignment.RightExpression);
            }
            catch (LaplaceExpressionException ex)
            {
                AddError(context, ex.Message, assignment.LineInfo, ex);
                return null;
            }
            catch (Exception ex)
            {
                AddError(
                    context,
                    "laplace transfer expression must be a rational polynomial in s",
                    assignment.LineInfo,
                    ex);
                return null;
            }

            return new LaplaceSourceDefinition(
                sourceName,
                parameters[0].Value,
                parameters[1].Value,
                assignment.LeftExpression,
                assignment.RightExpression,
                input,
                transferFunction,
                0.0,
                assignment.LineInfo);
        }

        private static bool HasOutputNodes(ParameterCollection parameters, IReadingContext context)
        {
            if (parameters.Count < 2 || !parameters.IsValueString(0) || !parameters.IsValueString(1))
            {
                AddError(context, "laplace expects output nodes before LAPLACE", parameters.LineInfo);
                return false;
            }

            return true;
        }

        private static string GetUnsupportedExtraParameterMessage(ParameterCollection extraParameters)
        {
            foreach (var parameter in extraParameters)
            {
                var assignmentParameter = parameter as AssignmentParameter;
                if (assignmentParameter != null)
                {
                    if (string.Equals(assignmentParameter.Name, "m", StringComparison.OrdinalIgnoreCase))
                    {
                        return "laplace multiplier M is not supported yet";
                    }

                    if (IsDelayName(assignmentParameter.Name))
                    {
                        return "laplace delay syntax is not supported yet";
                    }
                }

                var wordParameter = parameter as WordParameter;
                if (wordParameter != null && IsDelayName(wordParameter.Value))
                {
                    return "laplace delay syntax is not supported yet";
                }
            }

            return "laplace syntax variant is recognized but not supported yet";
        }

        private static bool IsDelayName(string name)
        {
            return string.Equals(name, "td", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "delay", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseVoltageInput(
            string expression,
            IReadingContext context,
            out LaplaceSourceInput input)
        {
            input = null;

            if (!TryParseRawVoltageInput(expression, out var positiveNode, out var negativeNode))
            {
                return false;
            }

            if (!IsVoltageInputAst(expression, positiveNode, negativeNode, context))
            {
                return false;
            }

            input = new LaplaceSourceInput(positiveNode, negativeNode);
            return true;
        }

        private static bool TryParseRawVoltageInput(
            string expression,
            out string positiveNode,
            out string negativeNode)
        {
            positiveNode = null;
            negativeNode = null;

            if (expression == null)
            {
                return false;
            }

            var text = expression.Trim();
            if (text.Length < 4 || text[0] != 'v' && text[0] != 'V')
            {
                return false;
            }

            var openParenthesisIndex = 1;
            while (openParenthesisIndex < text.Length && char.IsWhiteSpace(text[openParenthesisIndex]))
            {
                openParenthesisIndex++;
            }

            if (openParenthesisIndex >= text.Length || text[openParenthesisIndex] != '(')
            {
                return false;
            }

            var closeParenthesisIndex = text.Length - 1;
            if (text[closeParenthesisIndex] != ')')
            {
                return false;
            }

            if (text.IndexOf(')', openParenthesisIndex + 1) != closeParenthesisIndex
                || text.IndexOf('(', openParenthesisIndex + 1) >= 0)
            {
                return false;
            }

            var inner = text.Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);
            var parts = inner.Split(',');
            if (parts.Length != 1 && parts.Length != 2)
            {
                return false;
            }

            positiveNode = parts[0].Trim();
            negativeNode = parts.Length == 2 ? parts[1].Trim() : "0";

            return IsValidNodeName(positiveNode) && IsValidNodeName(negativeNode);
        }

        private static bool IsValidNodeName(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                return false;
            }

            foreach (var character in nodeName)
            {
                if (char.IsWhiteSpace(character))
                    return false;

                switch (character)
                {
                    case '+':
                    case '*':
                    case '/':
                    case '^':
                    case '(':
                    case ')':
                    case '{':
                    case '}':
                    case '@':
                    case ',':
                        return false;
                }
            }

            return true;
        }

        private static bool IsVoltageInputAst(
            string expression,
            string positiveNode,
            string negativeNode,
            IReadingContext context)
        {
            try
            {
                var node = ExpressionParser.Parse(Lexer.FromString(expression), true);
                var comparer = StringComparerProvider.Get(context.ReaderSettings.CaseSensitivity.IsNodeNameCaseSensitive);
                if (string.Equals(negativeNode, "0", StringComparison.Ordinal)
                    && node is VariableNode singleNode
                    && singleNode.NodeType == NodeTypes.Voltage)
                {
                    return comparer.Equals(singleNode.Name, positiveNode);
                }

                var binaryNode = node as BinaryOperatorNode;
                if (binaryNode == null || binaryNode.NodeType != NodeTypes.Subtract)
                {
                    return false;
                }

                var leftNode = binaryNode.Left as VariableNode;
                var rightNode = binaryNode.Right as VariableNode;

                return leftNode != null
                    && rightNode != null
                    && leftNode.NodeType == NodeTypes.Voltage
                    && rightNode.NodeType == NodeTypes.Voltage
                    && comparer.Equals(leftNode.Name, positiveNode)
                    && comparer.Equals(rightNode.Name, negativeNode);
            }
            catch
            {
                return false;
            }
        }

        private static void AddError(
            IReadingContext context,
            string message,
            SpiceLineInfo lineInfo,
            Exception exception = null)
        {
            context.Result.ValidationResult.AddError(ValidationEntrySource.Reader, message, lineInfo, exception);
        }
    }
}
