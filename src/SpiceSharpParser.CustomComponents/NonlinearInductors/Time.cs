using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.CustomComponents.NonlinearInductors
{
    /// <summary>
    /// Transient behavior for a <see cref="NonlinearInductor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class Time : Biasing,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly ElementSet<double> _timeElements;
        private readonly IDerivative _fluxState;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time" /> class.
        /// </summary>
        /// <param name="context">The component binding context.</param>
        public Time(IComponentBindingContext context)
            : base(context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();

            _timeElements = new ElementSet<double>(
                state.Solver,
                Variables.GetTimeMatrixLocations(state.Map),
                Variables.GetTimeRhsIndices(state.Map));
            _fluxState = method.CreateDerivative(true);
        }

        /// <summary>
        /// Gets the time-domain flux derivative.
        /// </summary>
        [ParameterName("dfluxdt"), ParameterInfo("The time derivative of flux", Units = "V")]
        public double FluxDerivative => _fluxState.Derivative;

        /// <inheritdoc />
        void ITimeBehavior.InitializeStates()
        {
            if (_time.UseIc && Parameters.InitialCondition.Given)
            {
                _fluxState.Value = EvaluateFlux(Parameters.InitialCondition);
                return;
            }

            _fluxState.Value = EvaluateFlux(Current);
        }

        /// <inheritdoc />
        void IBiasingBehavior.Load()
        {
            Load();

            if (_time.UseDc)
            {
                return;
            }

            double current = Current;
            double flux = EvaluateFlux(current);
            double inductance = EvaluateFluxDerivative(current);

            IncrementalInductance = inductance;
            _fluxState.Value = flux;
            _fluxState.Derive();

            JacobianInfo info = _fluxState.GetContributions(inductance, current);
            _timeElements.Add(-info.Jacobian, info.Rhs);
        }
    }
}
