using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using ExpressionParser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class LaplaceFunctionExpressionLowerer
    {
        private const string FunctionName = "laplace";
        private const string InputErrorMessage = "laplace input expression must be V(node), V(node1,node2), or I(source)";

        private readonly EvaluationContext _evaluationContext;
        private readonly Func<string, double> _evaluateDouble;
        private readonly Action<string, SpiceLineInfo, Exception> _addError;
        private readonly Func<string, string> _generateEntityName;
        private readonly Func<string, bool> _entityExists;
        private readonly SpiceLineInfo _lineInfo;
        private readonly BehavioralExpressionFormatter _formatter = new BehavioralExpressionFormatter();
        private readonly List<string> _errors = new List<string>();

        public LaplaceFunctionExpressionLowerer(
            EvaluationContext evaluationContext,
            Func<string, double> evaluateDouble,
            Action<string, SpiceLineInfo, Exception> addError,
            Func<string, string> generateEntityName,
            Func<string, bool> entityExists,
            SpiceLineInfo lineInfo)
        {
            _evaluationContext = evaluationContext ?? throw new ArgumentNullException(nameof(evaluationContext));
            _evaluateDouble = evaluateDouble ?? throw new ArgumentNullException(nameof(evaluateDouble));
            _addError = addError ?? throw new ArgumentNullException(nameof(addError));
            _generateEntityName = generateEntityName ?? (name => name);
            _entityExists = entityExists ?? (_ => false);
            _lineInfo = lineInfo;
        }

        public LaplaceFunctionLoweringResult Lower(
            string sourceName,
            string localSourceName,
            string outputPositiveNode,
            string outputNegativeNode,
            string expression,
            IEnumerable<Parameter> extraParameters,
            LaplaceOutputKind outputKind)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return LaplaceFunctionLoweringResult.NoMatch();
            }

            Node root;
            try
            {
                root = ExpressionParser.Parse(Lexer.FromString(expression), true);
            }
            catch (Exception ex)
            {
                if (ContainsLaplaceFunctionText(expression))
                {
                    AddError("laplace function expression could not be parsed", _lineInfo, ex);
                    return LaplaceFunctionLoweringResult.Error();
                }

                return LaplaceFunctionLoweringResult.NoMatch();
            }

            var calls = new List<FunctionNode>();
            CollectLaplaceCalls(root, calls);
            if (calls.Count == 0)
            {
                return LaplaceFunctionLoweringResult.NoMatch();
            }

            var parsedCalls = new List<ParsedLaplaceCall>();
            foreach (var call in calls)
            {
                if (!TryParseCall(call, out var parsedCall))
                {
                    return LaplaceFunctionLoweringResult.Error();
                }

                parsedCalls.Add(parsedCall);
            }

            var isDirect = IsLaplaceCall(root) && calls.Count == 1;
            if (!TryParseOptions(extraParameters ?? Enumerable.Empty<Parameter>(), calls.Count, outputKind, isDirect, out var options))
            {
                return LaplaceFunctionLoweringResult.Error();
            }

            if (isDirect)
            {
                var parsedCall = parsedCalls[0];
                var transferFunction = parsedCall.TransferFunction.ScaleNumerator(options.Multiplier);
                var definition = CreateDefinition(
                    sourceName,
                    outputPositiveNode,
                    outputNegativeNode,
                    parsedCall,
                    transferFunction,
                    options.Delay);

                return LaplaceFunctionLoweringResult.Direct(definition);
            }

            var helperDefinitions = new List<LaplaceFunctionCallDefinition>();
            var usedHelperNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < parsedCalls.Count; i++)
            {
                var helperNames = CreateHelperNames(localSourceName ?? sourceName, i, usedHelperNames);
                var definition = CreateDefinition(
                    helperNames.EntityName,
                    helperNames.NodeName,
                    "0",
                    parsedCalls[i],
                    parsedCalls[i].TransferFunction,
                    calls.Count == 1 ? options.Delay : 0.0);

                helperDefinitions.Add(new LaplaceFunctionCallDefinition(helperNames.NodeName, definition));
            }

            var rewrittenNode = ReplaceLaplaceCalls(root, calls, helperDefinitions);
            var rewrittenExpression = _formatter.Format(rewrittenNode);

            return LaplaceFunctionLoweringResult.Mixed(helperDefinitions, rewrittenExpression);
        }

        private bool TryParseCall(FunctionNode call, out ParsedLaplaceCall parsedCall)
        {
            parsedCall = null;

            if (call.Arguments.Count != 2)
            {
                AddError("laplace function expects exactly two arguments", _lineInfo, null);
                return false;
            }

            if (!LaplaceSourceParser.TryParseInput(call.Arguments[0], out var input))
            {
                AddError(InputErrorMessage, _lineInfo, null);
                return false;
            }

            LaplaceTransferFunction transferFunction;
            try
            {
                transferFunction = new LaplaceExpressionParser(_evaluationContext, lineInfo: _lineInfo)
                    .Parse(call.Arguments[1]);
            }
            catch (LaplaceExpressionException ex)
            {
                AddError(ex.Message, _lineInfo, ex);
                return false;
            }
            catch (Exception ex)
            {
                AddError("laplace transfer expression must be a rational polynomial in s", _lineInfo, ex);
                return false;
            }

            parsedCall = new ParsedLaplaceCall(
                _formatter.Format(call.Arguments[0]),
                _formatter.Format(call.Arguments[1]),
                input,
                transferFunction);
            return true;
        }

        private bool TryParseOptions(
            IEnumerable<Parameter> extraParameters,
            int callCount,
            LaplaceOutputKind outputKind,
            bool isDirect,
            out LaplaceFunctionOptions options)
        {
            options = new LaplaceFunctionOptions();
            var hasMultiplier = false;
            var hasDelay = false;

            foreach (var parameter in extraParameters)
            {
                if (parameter is AssignmentParameter assignmentParameter)
                {
                    if (IsName(assignmentParameter.Name, "m"))
                    {
                        if (hasMultiplier)
                        {
                            AddError("laplace multiplier M can be specified only once", assignmentParameter.LineInfo, null);
                            return false;
                        }

                        if (!TryEvaluateFiniteOption(
                            assignmentParameter,
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
                            AddError("laplace delay can be specified only once", assignmentParameter.LineInfo, null);
                            return false;
                        }

                        if (!TryEvaluateFiniteOption(
                            assignmentParameter,
                            "laplace delay must be a finite constant expression",
                            out var delay))
                        {
                            return false;
                        }

                        if (delay < 0.0)
                        {
                            AddError("laplace delay must be non-negative", assignmentParameter.LineInfo, null);
                            return false;
                        }

                        options.Delay = delay;
                        hasDelay = true;
                        continue;
                    }
                }

                if (parameter is WordParameter wordParameter
                    && (IsDelayName(wordParameter.Value) || IsName(wordParameter.Value, "m")))
                {
                    AddError("laplace options must use assignment syntax", wordParameter.LineInfo, null);
                    return false;
                }
            }

            if (hasDelay && callCount > 1)
            {
                AddError("laplace source-level delay options can be used only when one LAPLACE call is present", _lineInfo, null);
                return false;
            }

            if (hasMultiplier && !isDirect && outputKind == LaplaceOutputKind.Voltage)
            {
                AddError("laplace M option is not supported for mixed voltage-output expressions", _lineInfo, null);
                return false;
            }

            if (!isDirect)
            {
                options.Multiplier = 1.0;
            }

            return true;
        }

        private bool TryEvaluateFiniteOption(
            AssignmentParameter parameter,
            string errorMessage,
            out double value)
        {
            value = 0.0;

            try
            {
                value = _evaluateDouble(parameter.Value);
            }
            catch (Exception ex)
            {
                AddError(errorMessage, parameter.LineInfo, ex);
                return false;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                AddError(errorMessage, parameter.LineInfo, null);
                return false;
            }

            return true;
        }

        private LaplaceSourceDefinition CreateDefinition(
            string sourceName,
            string outputPositiveNode,
            string outputNegativeNode,
            ParsedLaplaceCall parsedCall,
            LaplaceTransferFunction transferFunction,
            double delay)
        {
            return new LaplaceSourceDefinition(
                sourceName,
                outputPositiveNode,
                outputNegativeNode,
                parsedCall.InputExpression,
                parsedCall.TransferExpression,
                parsedCall.Input,
                transferFunction,
                delay,
                _lineInfo);
        }

        private HelperNames CreateHelperNames(
            string sourceName,
            int index,
            ISet<string> usedHelperNames)
        {
            var sanitizedSourceName = Sanitize(sourceName);
            var nodeBaseName = "__ssp_laplace_" + sanitizedSourceName + "_" + index;
            var entityBaseName = nodeBaseName + "_src";
            var suffix = 0;

            while (true)
            {
                var localNodeName = suffix == 0 ? nodeBaseName : nodeBaseName + "_" + suffix;
                var localEntityName = suffix == 0 ? entityBaseName : entityBaseName + "_" + suffix;
                var entityName = _generateEntityName(localEntityName);

                if (!usedHelperNames.Contains(entityName) && !_entityExists(entityName))
                {
                    usedHelperNames.Add(entityName);
                    return new HelperNames(localNodeName, entityName);
                }

                suffix++;
            }
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "source";
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                if (character >= 'a' && character <= 'z'
                    || character >= 'A' && character <= 'Z'
                    || character >= '0' && character <= '9'
                    || character == '_')
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('_');
                }
            }

            return builder.Length == 0 ? "source" : builder.ToString();
        }

        private Node ReplaceLaplaceCalls(
            Node node,
            IReadOnlyList<FunctionNode> calls,
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions)
        {
            if (IsLaplaceCall(node))
            {
                for (var i = 0; i < calls.Count; i++)
                {
                    if (ReferenceEquals(node, calls[i]))
                    {
                        return Node.Voltage(helperDefinitions[i].HelperNodeName);
                    }
                }

                return node;
            }

            switch (node)
            {
                case UnaryOperatorNode unaryNode:
                    return ReplaceUnary(unaryNode, calls, helperDefinitions);

                case BinaryOperatorNode binaryNode:
                    return ReplaceBinary(binaryNode, calls, helperDefinitions);

                case TernaryOperatorNode ternaryNode:
                    return Node.Conditional(
                        ReplaceLaplaceCalls(ternaryNode.Condition, calls, helperDefinitions),
                        ReplaceLaplaceCalls(ternaryNode.IfTrue, calls, helperDefinitions),
                        ReplaceLaplaceCalls(ternaryNode.IfFalse, calls, helperDefinitions));

                case FunctionNode functionNode:
                    return Node.Function(
                        functionNode.Name,
                        functionNode.Arguments.Select(argument => ReplaceLaplaceCalls(argument, calls, helperDefinitions)).ToArray());

                default:
                    return node;
            }
        }

        private Node ReplaceUnary(
            UnaryOperatorNode node,
            IReadOnlyList<FunctionNode> calls,
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions)
        {
            var argument = ReplaceLaplaceCalls(node.Argument, calls, helperDefinitions);
            switch (node.NodeType)
            {
                case NodeTypes.Plus:
                    return Node.Plus(argument);

                case NodeTypes.Minus:
                    return Node.Minus(argument);

                case NodeTypes.Not:
                    return Node.Not(argument);

                default:
                    return node;
            }
        }

        private Node ReplaceBinary(
            BinaryOperatorNode node,
            IReadOnlyList<FunctionNode> calls,
            IReadOnlyList<LaplaceFunctionCallDefinition> helperDefinitions)
        {
            var left = ReplaceLaplaceCalls(node.Left, calls, helperDefinitions);
            var right = ReplaceLaplaceCalls(node.Right, calls, helperDefinitions);
            switch (node.NodeType)
            {
                case NodeTypes.Add:
                    return Node.Add(left, right);

                case NodeTypes.Subtract:
                    return Node.Subtract(left, right);

                case NodeTypes.Multiply:
                    return Node.Multiply(left, right);

                case NodeTypes.Divide:
                    return Node.Divide(left, right);

                case NodeTypes.Modulo:
                    return Node.Modulo(left, right);

                case NodeTypes.LessThan:
                    return Node.LessThan(left, right);

                case NodeTypes.GreaterThan:
                    return Node.GreaterThan(left, right);

                case NodeTypes.LessThanOrEqual:
                    return Node.LessThanOrEqual(left, right);

                case NodeTypes.GreaterThanOrEqual:
                    return Node.GreaterThanOrEqual(left, right);

                case NodeTypes.Equals:
                    return Node.Equals(left, right);

                case NodeTypes.NotEquals:
                    return Node.NotEquals(left, right);

                case NodeTypes.And:
                    return Node.And(left, right);

                case NodeTypes.Or:
                    return Node.Or(left, right);

                case NodeTypes.Xor:
                    return Node.Xor(left, right);

                case NodeTypes.Pow:
                    return Node.Power(left, right);

                default:
                    return node;
            }
        }

        private static void CollectLaplaceCalls(Node node, ICollection<FunctionNode> calls)
        {
            if (node == null)
            {
                return;
            }

            if (node is FunctionNode functionNode)
            {
                if (IsLaplaceCall(functionNode))
                {
                    calls.Add(functionNode);
                    return;
                }

                foreach (var argument in functionNode.Arguments)
                {
                    CollectLaplaceCalls(argument, calls);
                }

                return;
            }

            if (node is UnaryOperatorNode unaryNode)
            {
                CollectLaplaceCalls(unaryNode.Argument, calls);
                return;
            }

            if (node is BinaryOperatorNode binaryNode)
            {
                CollectLaplaceCalls(binaryNode.Left, calls);
                CollectLaplaceCalls(binaryNode.Right, calls);
                return;
            }

            if (node is TernaryOperatorNode ternaryNode)
            {
                CollectLaplaceCalls(ternaryNode.Condition, calls);
                CollectLaplaceCalls(ternaryNode.IfTrue, calls);
                CollectLaplaceCalls(ternaryNode.IfFalse, calls);
            }
        }

        private static bool IsLaplaceCall(Node node)
        {
            return node is FunctionNode functionNode
                && string.Equals(functionNode.Name, FunctionName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsLaplaceFunctionText(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            var index = 0;
            while (index < expression.Length)
            {
                index = expression.IndexOf(FunctionName, index, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    return false;
                }

                var nextIndex = index + FunctionName.Length;
                while (nextIndex < expression.Length && char.IsWhiteSpace(expression[nextIndex]))
                {
                    nextIndex++;
                }

                if (nextIndex < expression.Length && expression[nextIndex] == '(')
                {
                    return true;
                }

                index += FunctionName.Length;
            }

            return false;
        }

        private static bool IsDelayName(string name)
        {
            return string.Equals(name, "td", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "delay", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsName(string value, string expected)
        {
            return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
        }

        private void AddError(string message, SpiceLineInfo lineInfo, Exception exception)
        {
            _errors.Add(message);
            _addError(message, lineInfo, exception);
        }

        private sealed class ParsedLaplaceCall
        {
            public ParsedLaplaceCall(
                string inputExpression,
                string transferExpression,
                LaplaceSourceInput input,
                LaplaceTransferFunction transferFunction)
            {
                InputExpression = inputExpression;
                TransferExpression = transferExpression;
                Input = input;
                TransferFunction = transferFunction;
            }

            public string InputExpression { get; }

            public string TransferExpression { get; }

            public LaplaceSourceInput Input { get; }

            public LaplaceTransferFunction TransferFunction { get; }
        }

        private sealed class LaplaceFunctionOptions
        {
            public double Multiplier { get; set; } = 1.0;

            public double Delay { get; set; }
        }

        private sealed class HelperNames
        {
            public HelperNames(string nodeName, string entityName)
            {
                NodeName = nodeName;
                EntityName = entityName;
            }

            public string NodeName { get; }

            public string EntityName { get; }
        }
    }
}
