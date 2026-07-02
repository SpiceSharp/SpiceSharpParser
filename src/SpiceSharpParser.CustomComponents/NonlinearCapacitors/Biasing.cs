using SpiceSharp;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.NonlinearCapacitors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="NonlinearCapacitor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IParameterized<NonlinearCapacitorParameters>
    {
        private static readonly VariableNode VoltageVariable = Node.Variable("x");
        private readonly EvaluationVariable _voltageEvaluationVariable = new EvaluationVariable();
        private Func<double> _charge;
        private Func<double> _chargeDerivative;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            Parameters = context.GetParameterSet<NonlinearCapacitorParameters>();
            var state = context.GetState<IBiasingSimulationState>();

            Variables = new NonlinearCapacitorVariables<double>(state, context);
            BuildFunctions();
        }

        /// <inheritdoc />
        public NonlinearCapacitorParameters Parameters { get; }

        /// <summary>
        /// Gets the variables used by the behavior.
        /// </summary>
        protected NonlinearCapacitorVariables<double> Variables { get; }

        /// <summary>
        /// Gets the voltage across the nonlinear capacitor.
        /// </summary>
        [ParameterName("v"), ParameterName("vc"), ParameterInfo("The voltage across the nonlinear capacitor")]
        public double Voltage => Variables.Positive.Value - Variables.Negative.Value;

        /// <summary>
        /// Gets the current through the nonlinear capacitor.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The current through the nonlinear capacitor")]
        public virtual double Current => 0.0;

        /// <summary>
        /// Gets the total charge represented by the nonlinear capacitor.
        /// </summary>
        [ParameterName("q"), ParameterName("charge"), ParameterInfo("The charge", Units = "C")]
        public double Charge => EvaluateCharge(Voltage);

        /// <summary>
        /// Gets the operating-point incremental capacitance.
        /// </summary>
        [ParameterName("capacitance"), ParameterName("cap"), ParameterInfo("The incremental capacitance", Units = "F")]
        public double IncrementalCapacitance { get; protected set; }

        /// <summary>
        /// Gets the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("The instantaneous power")]
        public double Power => Voltage * Current;

        /// <inheritdoc />
        void IBiasingBehavior.Load()
        {
            Load();
        }

        /// <summary>
        /// Loads the DC open-circuit behavior.
        /// </summary>
        protected virtual void Load()
        {
            IncrementalCapacitance = EvaluateChargeDerivative(Voltage);
        }

        /// <summary>
        /// Evaluates total charge for the terminal voltage.
        /// </summary>
        /// <param name="voltage">The terminal voltage.</param>
        /// <returns>The total charge.</returns>
        protected double EvaluateCharge(double voltage)
        {
            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;

            _voltageEvaluationVariable.Value = voltage;
            return m * _charge() / n;
        }

        /// <summary>
        /// Evaluates the incremental capacitance for the terminal voltage.
        /// </summary>
        /// <param name="voltage">The terminal voltage.</param>
        /// <returns>The incremental capacitance.</returns>
        protected double EvaluateChargeDerivative(double voltage)
        {
            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;

            _voltageEvaluationVariable.Value = voltage;
            return m * _chargeDerivative() / n;
        }

        private void BuildFunctions()
        {
            if (string.IsNullOrWhiteSpace(Parameters.Expression))
            {
                throw new SpiceSharpException($"Charge expression is required for nonlinear capacitor '{Name}'.");
            }

            var chargeExpression = Parameters.ParseAction != null
                ? Parameters.ParseAction(Parameters.Expression)
                : Parser.Parse(Lexer.FromString(Parameters.Expression));

            var derivative = new Derivatives
            {
                Variables = new HashSet<VariableNode> { VoltageVariable },
                FunctionRules = DerivativesHelper.Defaults,
            };
            var derivativeExpression = derivative.Derive(chargeExpression)[VoltageVariable];

            _charge = BuildFunction(chargeExpression);
            _chargeDerivative = BuildFunction(derivativeExpression);
        }

        private Func<double> BuildFunction(Node expression)
        {
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (_, args) =>
            {
                if (args.Node.Equals(VoltageVariable))
                {
                    args.Variable = _voltageEvaluationVariable;
                }
            };

            return builder.Build(expression);
        }

        private sealed class EvaluationVariable : IVariable<double>
        {
            public string Name => "x";

            public IUnit Unit => Units.Volt;

            public double Value { get; set; }
        }
    }
}
