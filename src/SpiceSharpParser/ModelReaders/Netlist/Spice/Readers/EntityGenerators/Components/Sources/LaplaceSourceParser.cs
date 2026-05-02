using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
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
        private const string CurrentInputErrorMessage = "laplace input expression must be I(source)";
        private const string UnsupportedLaplaceFunctionMessage = "laplace function syntax is recognized but not supported yet";
        private static readonly IReadOnlyList<ILaplaceSyntaxRecognizer> SyntaxRecognizers =
            new ILaplaceSyntaxRecognizer[]
            {
                new CanonicalExpressionAssignmentRecognizer(),
                new NoEqualsExpressionPairRecognizer(),
                new EqualsExpressionPairRecognizer(),
                new UnsupportedKnownVariantRecognizer(),
            };

        public bool IsLaplaceSource(ParameterCollection parameters)
        {
            return TryGetLaplaceKeyword(parameters, out _);
        }

        public bool TryRejectUnsupportedLaplaceFunction(
            ParameterCollection parameters,
            IReadingContext context,
            params string[] assignmentNames)
        {
            var parameter = FindLaplaceFunctionParameter(parameters, assignmentNames);
            if (parameter == null)
            {
                return false;
            }

            AddError(context, UnsupportedLaplaceFunctionMessage, parameter.LineInfo);
            return true;
        }

        public LaplaceSourceDefinition ParseVoltageControlledSource(
            string sourceName,
            ParameterCollection parameters,
            IReadingContext context)
        {
            return ParseSource(
                sourceName,
                parameters,
                context.EvaluationContext,
                context.ReaderSettings.CaseSensitivity,
                context.Evaluator.EvaluateDouble,
                (message, lineInfo, exception) => AddError(context, message, lineInfo, exception),
                LaplaceSourceInputKind.Voltage);
        }

        public LaplaceSourceDefinition ParseCurrentControlledSource(
            string sourceName,
            ParameterCollection parameters,
            IReadingContext context)
        {
            return ParseSource(
                sourceName,
                parameters,
                context.EvaluationContext,
                context.ReaderSettings.CaseSensitivity,
                context.Evaluator.EvaluateDouble,
                (message, lineInfo, exception) => AddError(context, message, lineInfo, exception),
                LaplaceSourceInputKind.Current);
        }

        public LaplaceSourceDefinition ParseVoltageControlledSource(
            string sourceName,
            ParameterCollection parameters,
            EvaluationContext evaluationContext,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            Func<string, double> evaluateDouble,
            Action<string, SpiceLineInfo, Exception> addError)
        {
            return ParseSource(
                sourceName,
                parameters,
                evaluationContext,
                caseSettings,
                evaluateDouble,
                addError,
                LaplaceSourceInputKind.Voltage);
        }

        public LaplaceSourceDefinition ParseCurrentControlledSource(
            string sourceName,
            ParameterCollection parameters,
            EvaluationContext evaluationContext,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            Func<string, double> evaluateDouble,
            Action<string, SpiceLineInfo, Exception> addError)
        {
            return ParseSource(
                sourceName,
                parameters,
                evaluationContext,
                caseSettings,
                evaluateDouble,
                addError,
                LaplaceSourceInputKind.Current);
        }

        private LaplaceSourceDefinition ParseSource(
            string sourceName,
            ParameterCollection parameters,
            EvaluationContext evaluationContext,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            Func<string, double> evaluateDouble,
            Action<string, SpiceLineInfo, Exception> addError,
            LaplaceSourceInputKind expectedInputKind)
        {
            if (!IsLaplaceSource(parameters))
            {
                return null;
            }

            if (!HasOutputNodes(parameters, addError))
            {
                return null;
            }

            var syntax = RecognizeSyntax(parameters);
            if (!syntax.IsMatch)
            {
                addError("laplace syntax variant is recognized but not supported yet", parameters[2].LineInfo, null);
                return null;
            }

            if (syntax.ErrorMessage != null)
            {
                addError(syntax.ErrorMessage, syntax.LineInfo, null);
                return null;
            }

            if (!TryParseOptions(
                parameters.Skip(syntax.ExtraParameterStartIndex),
                evaluateDouble,
                addError,
                out var options))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(syntax.InputExpression))
            {
                addError("laplace expects input expression", syntax.LineInfo, null);
                return null;
            }

            if (string.IsNullOrWhiteSpace(syntax.TransferExpression))
            {
                addError("laplace expects transfer expression", syntax.LineInfo, null);
                return null;
            }

            if (!TryParseInput(syntax.InputExpression, expectedInputKind, caseSettings, out var input))
            {
                addError(
                    GetInputErrorMessage(syntax.InputExpression, expectedInputKind),
                    syntax.LineInfo,
                    null);
                return null;
            }

            LaplaceTransferFunction transferFunction;
            try
            {
                transferFunction = new LaplaceExpressionParser(
                    evaluationContext,
                    lineInfo: syntax.LineInfo).Parse(syntax.TransferExpression);
            }
            catch (LaplaceExpressionException ex)
            {
                addError(ex.Message, syntax.LineInfo, ex);
                return null;
            }
            catch (Exception ex)
            {
                addError(
                    "laplace transfer expression must be a rational polynomial in s",
                    syntax.LineInfo,
                    ex);
                return null;
            }

            transferFunction = transferFunction.ScaleNumerator(options.Multiplier);

            return new LaplaceSourceDefinition(
                sourceName,
                parameters[0].Value,
                parameters[1].Value,
                syntax.InputExpression,
                syntax.TransferExpression,
                input,
                transferFunction,
                options.Delay,
                syntax.LineInfo);
        }

        private static bool HasOutputNodes(
            ParameterCollection parameters,
            Action<string, SpiceLineInfo, Exception> addError)
        {
            if (parameters.Count < 2 || !parameters.IsValueString(0) || !parameters.IsValueString(1))
            {
                addError("laplace expects output nodes before LAPLACE", parameters.LineInfo, null);
                return false;
            }

            return true;
        }

        private static LaplaceSyntaxResult RecognizeSyntax(ParameterCollection parameters)
        {
            foreach (var recognizer in SyntaxRecognizers)
            {
                var result = recognizer.Recognize(parameters);
                if (result.IsMatch)
                {
                    return result;
                }
            }

            return LaplaceSyntaxResult.NoMatch;
        }

        private static bool TryParseOptions(
            IEnumerable<Parameter> extraParameters,
            Func<string, double> evaluateDouble,
            Action<string, SpiceLineInfo, Exception> addError,
            out LaplaceSourceOptions options)
        {
            options = new LaplaceSourceOptions();
            var hasMultiplier = false;
            var hasDelay = false;

            foreach (var parameter in extraParameters)
            {
                var assignmentParameter = parameter as AssignmentParameter;
                if (assignmentParameter != null)
                {
                    if (string.Equals(assignmentParameter.Name, "m", StringComparison.OrdinalIgnoreCase))
                    {
                        if (hasMultiplier)
                        {
                            addError("laplace multiplier M can be specified only once", assignmentParameter.LineInfo, null);
                            return false;
                        }

                        if (!TryEvaluateFiniteOption(
                            assignmentParameter,
                            evaluateDouble,
                            addError,
                            "laplace multiplier M must be a finite constant expression",
                            out var multiplier))
                        {
                            return false;
                        }

                        options.Multiplier = multiplier;
                        hasMultiplier = true;
                        continue;
                    }

                    if (IsDelayName(assignmentParameter.Name))
                    {
                        if (hasDelay)
                        {
                            addError("laplace delay can be specified only once", assignmentParameter.LineInfo, null);
                            return false;
                        }

                        if (!TryEvaluateFiniteOption(
                            assignmentParameter,
                            evaluateDouble,
                            addError,
                            "laplace delay must be a finite constant expression",
                            out var delay))
                        {
                            return false;
                        }

                        if (delay < 0.0)
                        {
                            addError("laplace delay must be non-negative", assignmentParameter.LineInfo, null);
                            return false;
                        }

                        options.Delay = delay;
                        hasDelay = true;
                        continue;
                    }

                    addError(
                        "laplace syntax variant is recognized but not supported yet",
                        assignmentParameter.LineInfo,
                        null);
                    return false;
                }

                var wordParameter = parameter as WordParameter;
                if (wordParameter != null && (IsDelayName(wordParameter.Value) || IsName(wordParameter.Value, "m")))
                {
                    addError("laplace options must use assignment syntax", wordParameter.LineInfo, null);
                    return false;
                }

                addError(
                    "laplace syntax variant is recognized but not supported yet",
                    parameter.LineInfo,
                    null);
                return false;
            }

            return true;
        }

        private static bool TryEvaluateFiniteOption(
            AssignmentParameter parameter,
            Func<string, double> evaluateDouble,
            Action<string, SpiceLineInfo, Exception> addError,
            string errorMessage,
            out double value)
        {
            value = 0.0;

            try
            {
                value = evaluateDouble(parameter.Value);
            }
            catch (Exception ex)
            {
                addError(errorMessage, parameter.LineInfo, ex);
                return false;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                addError(errorMessage, parameter.LineInfo, null);
                return false;
            }

            return true;
        }

        private static bool TryGetLaplaceKeyword(ParameterCollection parameters, out SpiceLineInfo lineInfo)
        {
            lineInfo = null;

            if (parameters == null || parameters.Count < 3)
            {
                return false;
            }

            var thirdParameter = parameters[2];
            if (IsLaplaceWord(thirdParameter))
            {
                if (parameters.Count == 3
                    || parameters[3] is ExpressionAssignmentParameter
                    || parameters[3] is ExpressionParameter
                    || parameters[3] is AssignmentParameter)
                {
                    lineInfo = thirdParameter.LineInfo;
                    return true;
                }
            }

            var assignmentParameter = thirdParameter as AssignmentParameter;
            if (assignmentParameter != null && IsName(assignmentParameter.Name, "laplace"))
            {
                lineInfo = assignmentParameter.LineInfo;
                return true;
            }

            return false;
        }

        private static bool IsLaplaceWord(Parameter parameter)
        {
            var wordParameter = parameter as WordParameter;
            return wordParameter != null && IsName(wordParameter.Value, "laplace");
        }

        private static Parameter FindLaplaceFunctionParameter(
            ParameterCollection parameters,
            string[] assignmentNames)
        {
            if (parameters == null || assignmentNames == null || assignmentNames.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                var assignmentParameter = parameters[i] as AssignmentParameter;
                if (assignmentParameter != null
                    && IsAnyName(assignmentParameter.Name, assignmentNames)
                    && assignmentParameter.Values.Any(ContainsLaplaceFunction))
                {
                    return assignmentParameter;
                }

                var wordParameter = parameters[i] as WordParameter;
                if (wordParameter != null
                    && IsAnyName(wordParameter.Value, assignmentNames)
                    && i + 1 < parameters.Count
                    && parameters[i + 1] is ExpressionParameter expressionParameter
                    && ContainsLaplaceFunction(expressionParameter.Value))
                {
                    return expressionParameter;
                }
            }

            return null;
        }

        private static bool ContainsLaplaceFunction(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            var index = 0;
            while (index < expression.Length)
            {
                index = expression.IndexOf("laplace", index, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    return false;
                }

                var nextIndex = index + "laplace".Length;
                while (nextIndex < expression.Length && char.IsWhiteSpace(expression[nextIndex]))
                {
                    nextIndex++;
                }

                if (nextIndex < expression.Length && expression[nextIndex] == '(')
                {
                    return true;
                }

                index += "laplace".Length;
            }

            return false;
        }

        private static bool IsDelayName(string name)
        {
            return string.Equals(name, "td", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "delay", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAnyName(string value, IEnumerable<string> names)
        {
            return names.Any(name => IsName(value, name));
        }

        private static bool IsName(string value, string expected)
        {
            return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseVoltageInput(
            string expression,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            out LaplaceSourceInput input)
        {
            input = null;

            if (!TryParseRawVoltageInput(expression, out var positiveNode, out var negativeNode))
            {
                return false;
            }

            if (!IsVoltageInputAst(expression, positiveNode, negativeNode, caseSettings))
            {
                return false;
            }

            input = LaplaceSourceInput.Voltage(positiveNode, negativeNode);
            return true;
        }

        internal static bool TryParseInput(
            Node node,
            out LaplaceSourceInput input)
        {
            input = null;

            if (node is VariableNode variableNode)
            {
                if (variableNode.NodeType == NodeTypes.Voltage
                    && IsValidNodeName(variableNode.Name))
                {
                    input = LaplaceSourceInput.Voltage(variableNode.Name, "0");
                    return true;
                }

                if (variableNode.NodeType == NodeTypes.Current
                    && IsValidNodeName(variableNode.Name))
                {
                    input = LaplaceSourceInput.Current(variableNode.Name);
                    return true;
                }
            }

            if (node is BinaryOperatorNode binaryNode
                && binaryNode.NodeType == NodeTypes.Subtract
                && binaryNode.Left is VariableNode leftNode
                && binaryNode.Right is VariableNode rightNode
                && leftNode.NodeType == NodeTypes.Voltage
                && rightNode.NodeType == NodeTypes.Voltage
                && IsValidNodeName(leftNode.Name)
                && IsValidNodeName(rightNode.Name))
            {
                input = LaplaceSourceInput.Voltage(leftNode.Name, rightNode.Name);
                return true;
            }

            return false;
        }

        internal static bool TryParseInput(
            Node node,
            LaplaceSourceInputKind expectedInputKind,
            out LaplaceSourceInput input)
        {
            if (!TryParseInput(node, out input))
            {
                return false;
            }

            if (input.Kind != expectedInputKind)
            {
                input = null;
                return false;
            }

            return true;
        }

        private static bool TryParseInput(
            string expression,
            LaplaceSourceInputKind expectedInputKind,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            out LaplaceSourceInput input)
        {
            if (expectedInputKind == LaplaceSourceInputKind.Voltage)
            {
                return TryParseVoltageInput(expression, caseSettings, out input);
            }

            return TryParseCurrentInput(expression, caseSettings, out input);
        }

        private static bool TryParseCurrentInput(
            string expression,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            out LaplaceSourceInput input)
        {
            input = null;

            if (!TryParseRawCurrentInput(expression, out var controllingSource))
            {
                return false;
            }

            if (!IsCurrentInputAst(expression, controllingSource, caseSettings))
            {
                return false;
            }

            input = LaplaceSourceInput.Current(controllingSource);
            return true;
        }

        private static string GetInputErrorMessage(
            string expression,
            LaplaceSourceInputKind expectedInputKind)
        {
            if (expectedInputKind == LaplaceSourceInputKind.Current)
            {
                return CurrentInputErrorMessage;
            }

            return IsDifferentialVoltageExpression(expression)
                ? InputErrorMessage + "; use V(node1,node2) for differential voltage input"
                : InputErrorMessage;
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

        private static bool TryParseRawCurrentInput(
            string expression,
            out string controllingSource)
        {
            controllingSource = null;

            if (expression == null)
            {
                return false;
            }

            var text = expression.Trim();
            if (text.Length < 4 || text[0] != 'i' && text[0] != 'I')
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
            if (inner.IndexOf(',') >= 0)
            {
                return false;
            }

            controllingSource = inner.Trim();
            return IsValidNodeName(controllingSource);
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
            SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            try
            {
                var node = ExpressionParser.Parse(Lexer.FromString(expression), true);
                var comparer = StringComparerProvider.Get(caseSettings.IsNodeNameCaseSensitive);
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

        private static bool IsDifferentialVoltageExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            try
            {
                var node = ExpressionParser.Parse(Lexer.FromString(expression), true);
                return node is BinaryOperatorNode binaryNode
                    && binaryNode.NodeType == NodeTypes.Subtract
                    && binaryNode.Left is VariableNode leftNode
                    && binaryNode.Right is VariableNode rightNode
                    && leftNode.NodeType == NodeTypes.Voltage
                    && rightNode.NodeType == NodeTypes.Voltage
                    && IsValidNodeName(leftNode.Name)
                    && IsValidNodeName(rightNode.Name);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsCurrentInputAst(
            string expression,
            string controllingSource,
            SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            try
            {
                var node = ExpressionParser.Parse(Lexer.FromString(expression), true);
                var comparer = StringComparerProvider.Get(caseSettings.IsEntityNamesCaseSensitive);

                return node is VariableNode currentNode
                    && currentNode.NodeType == NodeTypes.Current
                    && comparer.Equals(currentNode.Name, controllingSource);
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

        private interface ILaplaceSyntaxRecognizer
        {
            LaplaceSyntaxResult Recognize(ParameterCollection parameters);
        }

        private sealed class CanonicalExpressionAssignmentRecognizer : ILaplaceSyntaxRecognizer
        {
            public LaplaceSyntaxResult Recognize(ParameterCollection parameters)
            {
                if (!IsLaplaceWord(parameters[2])
                    || parameters.Count < 4
                    || !(parameters[3] is ExpressionAssignmentParameter assignment))
                {
                    return LaplaceSyntaxResult.NoMatch;
                }

                return LaplaceSyntaxResult.Match(
                    assignment.LeftExpression,
                    assignment.RightExpression,
                    4,
                    assignment.LineInfo);
            }
        }

        private sealed class NoEqualsExpressionPairRecognizer : ILaplaceSyntaxRecognizer
        {
            public LaplaceSyntaxResult Recognize(ParameterCollection parameters)
            {
                if (!IsLaplaceWord(parameters[2])
                    || parameters.Count < 4
                    || !(parameters[3] is ExpressionParameter inputExpression))
                {
                    return LaplaceSyntaxResult.NoMatch;
                }

                if (parameters.Count == 4)
                {
                    return LaplaceSyntaxResult.Error(
                        "laplace expects transfer expression",
                        inputExpression.LineInfo);
                }

                var transferExpression = parameters[4] as ExpressionParameter;
                if (transferExpression == null)
                {
                    return LaplaceSyntaxResult.Error(
                        "laplace expects transfer expression",
                        parameters[4].LineInfo);
                }

                return LaplaceSyntaxResult.Match(
                    inputExpression.Value,
                    transferExpression.Value,
                    5,
                    inputExpression.LineInfo);
            }
        }

        private sealed class EqualsExpressionPairRecognizer : ILaplaceSyntaxRecognizer
        {
            public LaplaceSyntaxResult Recognize(ParameterCollection parameters)
            {
                var assignment = parameters[2] as AssignmentParameter;
                if (assignment == null || !IsName(assignment.Name, "laplace"))
                {
                    return LaplaceSyntaxResult.NoMatch;
                }

                if (parameters.Count == 3)
                {
                    return LaplaceSyntaxResult.Error(
                        "laplace expects transfer expression",
                        assignment.LineInfo);
                }

                var transferExpression = parameters[3] as ExpressionParameter;
                if (transferExpression == null)
                {
                    return LaplaceSyntaxResult.Error(
                        "laplace expects transfer expression",
                        parameters[3].LineInfo);
                }

                return LaplaceSyntaxResult.Match(
                    assignment.Value,
                    transferExpression.Value,
                    4,
                    assignment.LineInfo);
            }
        }

        private sealed class UnsupportedKnownVariantRecognizer : ILaplaceSyntaxRecognizer
        {
            public LaplaceSyntaxResult Recognize(ParameterCollection parameters)
            {
                if (!TryGetLaplaceKeyword(parameters, out var lineInfo))
                {
                    return LaplaceSyntaxResult.NoMatch;
                }

                if (parameters.Count == 3)
                {
                    return LaplaceSyntaxResult.Error(
                        IsLaplaceWord(parameters[2])
                            ? "laplace expects input expression"
                            : "laplace expects transfer expression",
                        lineInfo);
                }

                return LaplaceSyntaxResult.Error(
                    "laplace syntax variant is recognized but not supported yet",
                    parameters[3].LineInfo);
            }
        }

        private sealed class LaplaceSyntaxResult
        {
            public static readonly LaplaceSyntaxResult NoMatch = new LaplaceSyntaxResult(false, null, null, 0, null, null);

            private LaplaceSyntaxResult(
                bool isMatch,
                string inputExpression,
                string transferExpression,
                int extraParameterStartIndex,
                SpiceLineInfo lineInfo,
                string errorMessage)
            {
                IsMatch = isMatch;
                InputExpression = inputExpression;
                TransferExpression = transferExpression;
                ExtraParameterStartIndex = extraParameterStartIndex;
                LineInfo = lineInfo;
                ErrorMessage = errorMessage;
            }

            public bool IsMatch { get; }

            public string InputExpression { get; }

            public string TransferExpression { get; }

            public int ExtraParameterStartIndex { get; }

            public SpiceLineInfo LineInfo { get; }

            public string ErrorMessage { get; }

            public static LaplaceSyntaxResult Match(
                string inputExpression,
                string transferExpression,
                int extraParameterStartIndex,
                SpiceLineInfo lineInfo)
            {
                return new LaplaceSyntaxResult(true, inputExpression, transferExpression, extraParameterStartIndex, lineInfo, null);
            }

            public static LaplaceSyntaxResult Error(string message, SpiceLineInfo lineInfo)
            {
                return new LaplaceSyntaxResult(true, null, null, 0, lineInfo, message);
            }
        }

        private sealed class LaplaceSourceOptions
        {
            public double Multiplier { get; set; } = 1.0;

            public double Delay { get; set; }
        }
    }
}
