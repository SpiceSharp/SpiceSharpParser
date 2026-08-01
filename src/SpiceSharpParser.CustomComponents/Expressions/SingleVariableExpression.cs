using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Expressions
{
    /// <summary>
    /// Compiles a real expression in <c>x</c> together with its symbolic derivative.
    /// </summary>
    /// <remarks>
    /// Nonlinear capacitors and inductors use the same expression machinery. Keeping
    /// it here leaves their behaviors focused on device scaling and solver stamping.
    /// </remarks>
    internal sealed class SingleVariableExpression
    {
        private static readonly VariableNode VariableNode = Node.Variable("x");
        private readonly ExpressionVariable _variable;
        private readonly Func<double> _value;
        private readonly Func<double> _derivative;

        public SingleVariableExpression(
            string expression,
            Func<string, Node> parse,
            IUnit inputUnit,
            string missingExpressionMessage)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new SpiceSharpException(missingExpressionMessage);
            }

            _variable = new ExpressionVariable(inputUnit);

            Node valueExpression = parse != null
                ? parse(expression)
                : Parser.Parse(Lexer.FromString(expression));
            var derivatives = new Derivatives
            {
                Variables = new HashSet<VariableNode> { VariableNode },
                FunctionRules = DerivativesHelper.Defaults,
            };

            // Derive returns null when the expression does not depend on x.
            Dictionary<VariableNode, Node> derivativeMap = derivatives.Derive(valueExpression);
            Node derivativeExpression =
                derivativeMap != null && derivativeMap.TryGetValue(VariableNode, out Node derivative)
                    ? derivative
                    : Node.Zero;

            _value = Build(valueExpression);
            _derivative = Build(derivativeExpression);
        }

        public double Evaluate(double input)
        {
            _variable.Value = input;
            return _value();
        }

        public double EvaluateDerivative(double input)
        {
            _variable.Value = input;
            return _derivative();
        }

        private Func<double> Build(Node expression)
        {
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (_, args) =>
            {
                if (args.Node.Equals(VariableNode))
                {
                    args.Variable = _variable;
                }
            };

            return builder.Build(expression);
        }

        private sealed class ExpressionVariable : IVariable<double>
        {
            public ExpressionVariable(IUnit unit)
            {
                Unit = unit;
            }

            public string Name => "x";

            public IUnit Unit { get; }

            public double Value { get; set; }
        }
    }
}
