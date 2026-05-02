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
        private const string InlineOptionSyntaxErrorMessage = "laplace inline options must use assignment syntax";

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

            if (!TryValidateSourceLevelOptionConflicts(parsedCalls, options, calls.Count, outputKind, isDirect))
            {
                return LaplaceFunctionLoweringResult.Error();
            }

            var usedHelperNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inputHelperDefinitions = new List<LaplaceFunctionInputHelperDefinition>();

            if (isDirect)
            {
                var parsedCall = parsedCalls[0];
                var transferFunction = parsedCall.TransferFunction;
                if (options.HasMultiplier)
                {
                    transferFunction = transferFunction.ScaleNumerator(options.Multiplier);
                }

                var input = ResolveInput(
                    parsedCall,
                    localSourceName ?? sourceName,
                    0,
                    usedHelperNames,
                    inputHelperDefinitions);
                var definition = CreateDefinition(
                    sourceName,
                    outputPositiveNode,
                    outputNegativeNode,
                    parsedCall,
                    input,
                    transferFunction,
                    parsedCall.HasDelay ? parsedCall.Delay : options.Delay);

                return LaplaceFunctionLoweringResult.Direct(inputHelperDefinitions, definition);
            }

            var helperDefinitions = new List<LaplaceFunctionCallDefinition>();
            for (var i = 0; i < parsedCalls.Count; i++)
            {
                var parsedCall = parsedCalls[i];
                var input = ResolveInput(
                    parsedCall,
                    localSourceName ?? sourceName,
                    i,
                    usedHelperNames,
                    inputHelperDefinitions);
                var helperNames = CreateHelperNames(
                    localSourceName ?? sourceName,
                    i,
                    "__ssp_laplace_",
                    usedHelperNames);
                var definition = CreateDefinition(
                    helperNames.EntityName,
                    helperNames.NodeName,
                    "0",
                    parsedCall,
                    input,
                    parsedCall.TransferFunction,
                    parsedCall.HasDelay
                        ? parsedCall.Delay
                        : calls.Count == 1 ? options.Delay : 0.0);

                helperDefinitions.Add(new LaplaceFunctionCallDefinition(helperNames.NodeName, definition));
            }

            var rewrittenNode = ReplaceLaplaceCalls(root, calls, helperDefinitions);
            var rewrittenExpression = _formatter.Format(rewrittenNode);

            return LaplaceFunctionLoweringResult.Mixed(inputHelperDefinitions, helperDefinitions, rewrittenExpression);
        }

        private bool TryParseCall(FunctionNode call, out ParsedLaplaceCall parsedCall)
        {
            parsedCall = null;

            if (call.Arguments.Count < 2)
            {
                AddError("laplace function expects at least two arguments", _lineInfo, null);
                return false;
            }

            if (ContainsLaplaceCall(call.Arguments[0]))
            {
                AddError("laplace input expression cannot contain nested LAPLACE calls", _lineInfo, null);
                return false;
            }

            if (!TryParseInlineOptions(call.Arguments.Skip(2), out var options))
            {
                return false;
            }

            LaplaceSourceInput input = null;
            var requiresInputHelper = !LaplaceSourceParser.TryParseInput(call.Arguments[0], out input);

            LaplaceTransferFunction transferFunction;
            try
            {
                transferFunction = new LaplaceExpressionParser(_evaluationContext, lineInfo: _lineInfo)
                    .Parse(call.Arguments[1]);
                transferFunction = transferFunction.ScaleNumerator(options.Multiplier);
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
                requiresInputHelper,
                transferFunction,
                options.HasMultiplier,
                options.HasDelay,
                options.Delay);
            return true;
        }

        private bool TryParseInlineOptions(
            IEnumerable<Node> optionNodes,
            out LaplaceFunctionOptions options)
        {
            options = new LaplaceFunctionOptions();

            foreach (var optionNode in optionNodes)
            {
                if (!TryParseOptionAssignment(optionNode, out var name, out var valueNode))
                {
                    AddError(InlineOptionSyntaxErrorMessage, _lineInfo, null);
                    return false;
                }

                var valueExpression = _formatter.Format(valueNode);
                if (IsName(name, "m"))
                {
                    if (options.HasMultiplier)
                    {
                        AddError("laplace multiplier M can be specified only once", _lineInfo, null);
                        return false;
                    }

                    if (!TryEvaluateFiniteOption(
                        valueExpression,
                        _lineInfo,
                        "laplace multiplier M must be a finite constant expression",
                        out var multiplier))
                    {
                        return false;
                    }

                    options.Multiplier = multiplier;
                    options.HasMultiplier = true;
                    continue;
                }

                if (IsDelayName(name))
                {
                    if (options.HasDelay)
                    {
                        AddError("laplace delay can be specified only once", _lineInfo, null);
                        return false;
                    }

                    if (!TryEvaluateFiniteOption(
                        valueExpression,
                        _lineInfo,
                        "laplace delay must be a finite constant expression",
                        out var delay))
                    {
                        return false;
                    }

                    if (delay < 0.0)
                    {
                        AddError("laplace delay must be non-negative", _lineInfo, null);
                        return false;
                    }

                    options.Delay = delay;
                    options.HasDelay = true;
                    continue;
                }

                AddError("unknown laplace inline option '" + name + "'", _lineInfo, null);
                return false;
            }

            return true;
        }

        private static bool TryParseOptionAssignment(
            Node optionNode,
            out string name,
            out Node valueNode)
        {
            name = null;
            valueNode = null;

            if (optionNode is BinaryOperatorNode binaryNode
                && binaryNode.NodeType == NodeTypes.Equals
                && binaryNode.Left is VariableNode variableNode
                && variableNode.NodeType == NodeTypes.Variable)
            {
                name = variableNode.Name;
                valueNode = binaryNode.Right;
                return !string.IsNullOrWhiteSpace(name) && valueNode != null;
            }

            return false;
        }

        private bool TryParseOptions(
            IEnumerable<Parameter> extraParameters,
            int callCount,
            LaplaceOutputKind outputKind,
            bool isDirect,
            out LaplaceFunctionOptions options)
        {
            options = new LaplaceFunctionOptions();

            foreach (var parameter in extraParameters)
            {
                if (parameter is AssignmentParameter assignmentParameter)
                {
                    if (IsName(assignmentParameter.Name, "m"))
                    {
                        if (options.HasMultiplier)
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
                        options.HasMultiplier = true;
                        continue;
                    }

                    if (IsDelayName(assignmentParameter.Name))
                    {
                        if (options.HasDelay)
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
                        options.HasDelay = true;
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

            if (options.HasDelay && callCount > 1)
            {
                AddError("laplace source-level delay options can be used only when one LAPLACE call is present; move delay into each LAPLACE(...) call", _lineInfo, null);
                return false;
            }

            if (options.HasMultiplier && !isDirect && outputKind == LaplaceOutputKind.Voltage)
            {
                AddError("laplace M option is not supported for mixed voltage-output expressions", _lineInfo, null);
                return false;
            }

            return true;
        }

        private bool TryValidateSourceLevelOptionConflicts(
            IReadOnlyList<ParsedLaplaceCall> parsedCalls,
            LaplaceFunctionOptions sourceLevelOptions,
            int callCount,
            LaplaceOutputKind outputKind,
            bool isDirect)
        {
            if (sourceLevelOptions.HasDelay && parsedCalls.Any(call => call.HasDelay))
            {
                AddError("laplace delay can be specified only once", _lineInfo, null);
                return false;
            }

            if (sourceLevelOptions.HasDelay && callCount > 1)
            {
                AddError("laplace source-level delay options can be used only when one LAPLACE call is present; move delay into each LAPLACE(...) call", _lineInfo, null);
                return false;
            }

            if (sourceLevelOptions.HasMultiplier
                && isDirect
                && parsedCalls.Count == 1
                && parsedCalls[0].HasMultiplier)
            {
                AddError("laplace multiplier M can be specified only once", _lineInfo, null);
                return false;
            }

            if (sourceLevelOptions.HasMultiplier && !isDirect && outputKind == LaplaceOutputKind.Voltage)
            {
                AddError("laplace M option is not supported for mixed voltage-output expressions", _lineInfo, null);
                return false;
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

        private bool TryEvaluateFiniteOption(
            string expression,
            SpiceLineInfo lineInfo,
            string errorMessage,
            out double value)
        {
            value = 0.0;

            try
            {
                value = _evaluateDouble(expression);
            }
            catch (Exception ex)
            {
                AddError(errorMessage, lineInfo, ex);
                return false;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                AddError(errorMessage, lineInfo, null);
                return false;
            }

            return true;
        }

        private LaplaceSourceInput ResolveInput(
            ParsedLaplaceCall parsedCall,
            string sourceName,
            int index,
            ISet<string> usedHelperNames,
            ICollection<LaplaceFunctionInputHelperDefinition> inputHelperDefinitions)
        {
            if (!parsedCall.RequiresInputHelper)
            {
                return parsedCall.Input;
            }

            var helperNames = CreateHelperNames(
                sourceName,
                index,
                "__ssp_laplace_input_",
                usedHelperNames);
            inputHelperDefinitions.Add(new LaplaceFunctionInputHelperDefinition(
                helperNames.NodeName,
                helperNames.EntityName,
                parsedCall.InputExpression,
                _lineInfo));

            return LaplaceSourceInput.Voltage(helperNames.NodeName, "0");
        }

        private LaplaceSourceDefinition CreateDefinition(
            string sourceName,
            string outputPositiveNode,
            string outputNegativeNode,
            ParsedLaplaceCall parsedCall,
            LaplaceSourceInput input,
            LaplaceTransferFunction transferFunction,
            double delay)
        {
            return new LaplaceSourceDefinition(
                sourceName,
                outputPositiveNode,
                outputNegativeNode,
                parsedCall.InputExpression,
                parsedCall.TransferExpression,
                input,
                transferFunction,
                delay,
                _lineInfo);
        }

        private HelperNames CreateHelperNames(
            string sourceName,
            int index,
            string prefix,
            ISet<string> usedHelperNames)
        {
            var sanitizedSourceName = Sanitize(sourceName);
            var nodeBaseName = prefix + sanitizedSourceName + "_" + index;
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

        private static bool ContainsLaplaceCall(Node node)
        {
            if (node == null)
            {
                return false;
            }

            if (IsLaplaceCall(node))
            {
                return true;
            }

            if (node is FunctionNode functionNode)
            {
                return functionNode.Arguments.Any(ContainsLaplaceCall);
            }

            if (node is UnaryOperatorNode unaryNode)
            {
                return ContainsLaplaceCall(unaryNode.Argument);
            }

            if (node is BinaryOperatorNode binaryNode)
            {
                return ContainsLaplaceCall(binaryNode.Left) || ContainsLaplaceCall(binaryNode.Right);
            }

            if (node is TernaryOperatorNode ternaryNode)
            {
                return ContainsLaplaceCall(ternaryNode.Condition)
                    || ContainsLaplaceCall(ternaryNode.IfTrue)
                    || ContainsLaplaceCall(ternaryNode.IfFalse);
            }

            return false;
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
                bool requiresInputHelper,
                LaplaceTransferFunction transferFunction,
                bool hasMultiplier,
                bool hasDelay,
                double delay)
            {
                InputExpression = inputExpression;
                TransferExpression = transferExpression;
                Input = input;
                RequiresInputHelper = requiresInputHelper;
                TransferFunction = transferFunction;
                HasMultiplier = hasMultiplier;
                HasDelay = hasDelay;
                Delay = delay;
            }

            public string InputExpression { get; }

            public string TransferExpression { get; }

            public LaplaceSourceInput Input { get; }

            public bool RequiresInputHelper { get; }

            public LaplaceTransferFunction TransferFunction { get; }

            public bool HasMultiplier { get; }

            public bool HasDelay { get; }

            public double Delay { get; }
        }

        private sealed class LaplaceFunctionOptions
        {
            public double Multiplier { get; set; } = 1.0;

            public double Delay { get; set; }

            public bool HasMultiplier { get; set; }

            public bool HasDelay { get; set; }
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
