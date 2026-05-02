using System;
using System.Collections.Generic;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.Models.Netlist.Spice;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Laplace
{
    internal sealed class LaplaceExpressionParser
    {
        private static readonly HashSet<string> StochasticFunctionNames = new HashSet<string>(
            new[] { "agauss", "aunif", "flat", "gauss", "mc", "random", "unif" },
            StringComparer.OrdinalIgnoreCase);

        private readonly EvaluationContext _context;
        private readonly LaplaceExpressionOptions _options;
        private readonly SpiceLineInfo _lineInfo;

        public LaplaceExpressionParser(
            EvaluationContext context,
            LaplaceExpressionOptions options = null,
            SpiceLineInfo lineInfo = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? new LaplaceExpressionOptions();
            _lineInfo = lineInfo;
        }

        public LaplaceTransferFunction Parse(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            try
            {
                var node = Parser.Parse(Lexer.FromString(expression), true);
                return Parse(node);
            }
            catch (LaplaceExpressionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LaplaceExpressionException(
                    "laplace transfer expression must be a rational polynomial in s",
                    ex,
                    _lineInfo);
            }
        }

        public LaplaceTransferFunction Parse(Node node)
        {
            try
            {
                var rational = Build(node).Normalize(_options.ZeroTolerance, _options.RelativeTolerance);
                ValidateTransfer(rational);

                return new LaplaceTransferFunction(rational.Numerator, rational.Denominator);
            }
            catch (LaplaceExpressionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LaplaceExpressionException(
                    "laplace transfer expression must be a rational polynomial in s",
                    ex,
                    _lineInfo);
            }
        }

        private RationalPolynomial Build(Node node)
        {
            if (node == null)
            {
                throw CreateException("laplace transfer expression must be a rational polynomial in s");
            }

            if (node is ConstantNode constantNode)
            {
                return RationalPolynomial.FromConstant(RequireFinite(constantNode.Literal));
            }

            if (node is VariableNode variableNode)
            {
                return BuildVariable(variableNode);
            }

            if (node is UnaryOperatorNode unaryNode)
            {
                return BuildUnary(unaryNode);
            }

            if (node is BinaryOperatorNode binaryNode)
            {
                return BuildBinary(binaryNode);
            }

            if (node is FunctionNode functionNode)
            {
                if (StochasticFunctionNames.Contains(functionNode.Name))
                {
                    throw CreateException("laplace transfer coefficients must be constant expressions");
                }

                throw CreateException("laplace transfer expression must be a rational polynomial in s");
            }

            if (node is PropertyNode || node is TernaryOperatorNode)
            {
                throw CreateException("laplace transfer expression must be a rational polynomial in s");
            }

            throw CreateException("laplace transfer expression must be a rational polynomial in s");
        }

        private RationalPolynomial BuildVariable(VariableNode node)
        {
            if (node.NodeType == NodeTypes.Voltage || node.NodeType == NodeTypes.Current || node.NodeType == NodeTypes.Property)
            {
                throw CreateException("laplace transfer expression must be a rational polynomial in s");
            }

            if (IsLaplaceSymbol(node.Name))
            {
                if (HasReservedParameterName())
                {
                    throw CreateException("laplace transfer expression reserves symbol 's'; use a different parameter name");
                }

                return RationalPolynomial.SymbolS;
            }

            if (!_context.Parameters.TryGetValue(node.Name, out var parameter))
            {
                throw CreateException("laplace transfer coefficients must be constant expressions");
            }

            if (!parameter.CanProvideValueDirectly)
            {
                var parameterNode = Parser.Parse(Lexer.FromString(parameter.ValueExpression), true);
                if (ContainsLaplaceSymbol(parameterNode))
                {
                    throw CreateException("laplace transfer expression reserves symbol 's'; use a different parameter name");
                }

                if (ContainsProbe(parameterNode))
                {
                    throw CreateException("laplace transfer expression must be a rational polynomial in s");
                }

                if (ContainsStochasticFunction(parameterNode))
                {
                    throw CreateException("laplace transfer coefficients must be constant expressions");
                }
            }

            return RationalPolynomial.FromConstant(RequireFinite(_context.Evaluator.EvaluateDouble(parameter)));
        }

        private RationalPolynomial BuildUnary(UnaryOperatorNode node)
        {
            switch (node.NodeType)
            {
                case NodeTypes.Plus:
                    return Build(node.Argument);

                case NodeTypes.Minus:
                    return Build(node.Argument).Scale(-1.0);

                default:
                    throw CreateException("laplace transfer expression must be a rational polynomial in s");
            }
        }

        private RationalPolynomial BuildBinary(BinaryOperatorNode node)
        {
            switch (node.NodeType)
            {
                case NodeTypes.Add:
                    return Build(node.Left).Add(Build(node.Right));

                case NodeTypes.Subtract:
                    return Build(node.Left).Subtract(Build(node.Right));

                case NodeTypes.Multiply:
                    return Build(node.Left).Multiply(Build(node.Right));

                case NodeTypes.Divide:
                    return Build(node.Left).Divide(Build(node.Right));

                case NodeTypes.Pow:
                    return Build(node.Left).Pow(GetPowerExponent(node.Right));

                default:
                    throw CreateException("laplace transfer expression must be a rational polynomial in s");
            }
        }

        private int GetPowerExponent(Node exponentNode)
        {
            var exponent = Build(exponentNode).Normalize(_options.ZeroTolerance, _options.RelativeTolerance);
            if (exponent.Denominator.Degree != 0 || exponent.Numerator.Degree != 0)
            {
                throw CreateException("laplace transfer powers must be non-negative integers");
            }

            var value = exponent.Numerator.Coefficients[0] / exponent.Denominator.Coefficients[0];
            RequireFinite(value);
            var rounded = Math.Round(value);
            if (Math.Abs(value - rounded) > 1e-12 || rounded < 0.0 || rounded > int.MaxValue)
            {
                throw CreateException("laplace transfer powers must be non-negative integers");
            }

            return (int)rounded;
        }

        private void ValidateTransfer(RationalPolynomial rational)
        {
            if (rational.Numerator.IsZero)
            {
                return;
            }

            if (rational.Denominator.IsZero)
            {
                throw CreateException("laplace transfer denominator cannot be zero");
            }

            if (rational.Numerator.Degree > rational.Denominator.Degree)
            {
                throw CreateException("laplace transfer function is improper; numerator degree exceeds denominator degree");
            }

            if (IsZero(rational.Denominator.Coefficients[0]))
            {
                throw CreateException("laplace transfer function has singular DC gain");
            }

            if (rational.Denominator.Degree > _options.MaxOrder || rational.Numerator.Degree > _options.MaxOrder)
            {
                throw CreateException("laplace transfer function order exceeds " + _options.MaxOrder);
            }
        }

        private bool ContainsLaplaceSymbol(Node node)
        {
            if (node is VariableNode variableNode)
            {
                return variableNode.NodeType == NodeTypes.Variable && IsLaplaceSymbol(variableNode.Name);
            }

            if (node is UnaryOperatorNode unaryNode)
            {
                return ContainsLaplaceSymbol(unaryNode.Argument);
            }

            if (node is BinaryOperatorNode binaryNode)
            {
                return ContainsLaplaceSymbol(binaryNode.Left) || ContainsLaplaceSymbol(binaryNode.Right);
            }

            if (node is FunctionNode functionNode)
            {
                foreach (var argument in functionNode.Arguments)
                {
                    if (ContainsLaplaceSymbol(argument))
                    {
                        return true;
                    }
                }
            }

            if (node is TernaryOperatorNode ternaryNode)
            {
                return ContainsLaplaceSymbol(ternaryNode.Condition)
                    || ContainsLaplaceSymbol(ternaryNode.IfTrue)
                    || ContainsLaplaceSymbol(ternaryNode.IfFalse);
            }

            return false;
        }

        private bool ContainsProbe(Node node)
        {
            if (node is VariableNode variableNode)
            {
                return variableNode.NodeType == NodeTypes.Voltage
                    || variableNode.NodeType == NodeTypes.Current
                    || variableNode.NodeType == NodeTypes.Property;
            }

            if (node is PropertyNode)
            {
                return true;
            }

            if (node is UnaryOperatorNode unaryNode)
            {
                return ContainsProbe(unaryNode.Argument);
            }

            if (node is BinaryOperatorNode binaryNode)
            {
                return ContainsProbe(binaryNode.Left) || ContainsProbe(binaryNode.Right);
            }

            if (node is FunctionNode functionNode)
            {
                foreach (var argument in functionNode.Arguments)
                {
                    if (ContainsProbe(argument))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (node is TernaryOperatorNode ternaryNode)
            {
                return ContainsProbe(ternaryNode.Condition)
                    || ContainsProbe(ternaryNode.IfTrue)
                    || ContainsProbe(ternaryNode.IfFalse);
            }

            return false;
        }

        private bool ContainsStochasticFunction(Node node)
        {
            if (node is FunctionNode functionNode)
            {
                if (StochasticFunctionNames.Contains(functionNode.Name))
                {
                    return true;
                }

                foreach (var argument in functionNode.Arguments)
                {
                    if (ContainsStochasticFunction(argument))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (node is UnaryOperatorNode unaryNode)
            {
                return ContainsStochasticFunction(unaryNode.Argument);
            }

            if (node is BinaryOperatorNode binaryNode)
            {
                return ContainsStochasticFunction(binaryNode.Left) || ContainsStochasticFunction(binaryNode.Right);
            }

            if (node is TernaryOperatorNode ternaryNode)
            {
                return ContainsStochasticFunction(ternaryNode.Condition)
                    || ContainsStochasticFunction(ternaryNode.IfTrue)
                    || ContainsStochasticFunction(ternaryNode.IfFalse);
            }

            return false;
        }

        private bool HasReservedParameterName()
        {
            foreach (var parameterName in _context.Parameters.Keys)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(parameterName, "s"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLaplaceSymbol(string name)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(name, "s");
        }

        private bool IsZero(double value)
        {
            return Math.Abs(value) <= _options.ZeroTolerance;
        }

        private double RequireFinite(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw CreateException("laplace transfer coefficients must be finite");
            }

            return value;
        }

        private LaplaceExpressionException CreateException(string message)
        {
            return new LaplaceExpressionException(message, _lineInfo);
        }
    }
}
