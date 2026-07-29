using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.Expressions;

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
        private readonly ElementSet<double> _elements;
        private readonly SingleVariableExpression _fluxExpression;

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

            _fluxExpression = new SingleVariableExpression(
                Parameters.Expression,
                Parameters.ParseAction,
                Units.Ampere,
                $"Flux expression is required for nonlinear inductor '{Name}'.");
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

            return n * _fluxExpression.Evaluate(current / m);
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

            return n * _fluxExpression.EvaluateDerivative(current / m) / m;
        }
    }
}
