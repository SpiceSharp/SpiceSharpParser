using SpiceSharp;
using SpiceSharp.Algebra;
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

namespace SpiceSharpParser.CustomComponents.NonlinearInductors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="NonlinearInductor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class Biasing : Behavior,
        IBiasingBehavior,
        IBranchedBehavior<double>,
        IParameterized<NonlinearInductorParameters>
    {
        private static readonly VariableNode CurrentVariable = Node.Variable("x");
        private readonly ElementSet<double> _elements;
        private readonly EvaluationVariable _currentEvaluationVariable = new EvaluationVariable();
        private Func<double> _flux;
        private Func<double> _fluxDerivative;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.CheckNodes(2);

            Parameters = context.GetParameterSet<NonlinearInductorParameters>();
            var state = context.GetState<IBiasingSimulationState>();

            Variables = new NonlinearInductorVariables<double>(Name, state, context);
            Branch = Variables.Branch;
            _elements = new ElementSet<double>(
                state.Solver,
                Variables.GetBiasingMatrixLocations(state.Map));

            BuildFunctions();
        }

        /// <inheritdoc />
        public NonlinearInductorParameters Parameters { get; }

        /// <inheritdoc />
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Gets the variables used by the behavior.
        /// </summary>
        protected NonlinearInductorVariables<double> Variables { get; }

        /// <summary>
        /// Gets the voltage across the nonlinear inductor.
        /// </summary>
        [ParameterName("v"), ParameterName("vl"), ParameterInfo("The voltage across the nonlinear inductor")]
        public double Voltage => Variables.Positive.Value - Variables.Negative.Value;

        /// <summary>
        /// Gets the current through the nonlinear inductor.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("The current through the nonlinear inductor")]
        public double Current => Branch.Value;

        /// <summary>
        /// Gets the total flux linkage represented by the nonlinear inductor.
        /// </summary>
        [ParameterName("flux"), ParameterInfo("The flux linkage", Units = "Wb")]
        public double Flux => EvaluateFlux(Current);

        /// <summary>
        /// Gets the operating-point incremental inductance.
        /// </summary>
        [ParameterName("l"), ParameterName("inductance"), ParameterInfo("The incremental inductance", Units = "H")]
        public double IncrementalInductance { get; protected set; }

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
        /// Loads the DC branch equation.
        /// </summary>
        protected virtual void Load()
        {
            IncrementalInductance = EvaluateFluxDerivative(Current);
            _elements.Add(1.0, -1.0, -1.0, 1.0);
        }

        /// <summary>
        /// Evaluates total flux linkage for the terminal current.
        /// </summary>
        /// <param name="current">The terminal current.</param>
        /// <returns>The total flux linkage.</returns>
        protected double EvaluateFlux(double current)
        {
            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;

            _currentEvaluationVariable.Value = current / m;
            return n * _flux();
        }

        /// <summary>
        /// Evaluates the incremental inductance for the terminal current.
        /// </summary>
        /// <param name="current">The terminal current.</param>
        /// <returns>The incremental inductance.</returns>
        protected double EvaluateFluxDerivative(double current)
        {
            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;

            _currentEvaluationVariable.Value = current / m;
            return n * _fluxDerivative() / m;
        }

        private void BuildFunctions()
        {
            if (string.IsNullOrWhiteSpace(Parameters.Expression))
            {
                throw new SpiceSharpException($"Flux expression is required for nonlinear inductor '{Name}'.");
            }

            var fluxExpression = Parameters.ParseAction != null
                ? Parameters.ParseAction(Parameters.Expression)
                : Parser.Parse(Lexer.FromString(Parameters.Expression));

            var derivative = new Derivatives
            {
                Variables = new HashSet<VariableNode> { CurrentVariable },
                FunctionRules = DerivativesHelper.Defaults,
            };
            var derivativeExpression = derivative.Derive(fluxExpression)[CurrentVariable];

            _flux = BuildFunction(fluxExpression);
            _fluxDerivative = BuildFunction(derivativeExpression);
        }

        private Func<double> BuildFunction(Node expression)
        {
            var builder = new RealFunctionBuilder();
            builder.VariableFound += (_, args) =>
            {
                if (args.Node.Equals(CurrentVariable))
                {
                    args.Variable = _currentEvaluationVariable;
                }
            };

            return builder.Build(expression);
        }

        private sealed class EvaluationVariable : IVariable<double>
        {
            public string Name => "x";

            public IUnit Unit => Units.Ampere;

            public double Value { get; set; }
        }
    }
}
