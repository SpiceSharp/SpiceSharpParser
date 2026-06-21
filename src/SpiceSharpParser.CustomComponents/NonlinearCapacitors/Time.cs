using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.CustomComponents.NonlinearCapacitors
{
    /// <summary>
    /// Transient behavior for a <see cref="NonlinearCapacitor" />.
    /// </summary>
    [GeneratedParameters]
    public partial class Time : Biasing,
        IBiasingBehavior,
        ITimeBehavior
    {
        private readonly ElementSet<double> _timeElements;
        private readonly IDerivative _chargeState;
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
                Variables.GetMatrixLocations(state.Map),
                Variables.GetRhsIndices(state.Map));
            _chargeState = method.CreateDerivative(true);
        }

        /// <summary>
        /// Gets the time-domain charge derivative.
        /// </summary>
        [ParameterName("dqdt"), ParameterInfo("The time derivative of charge", Units = "A")]
        public double ChargeDerivative => _chargeState.Derivative;

        /// <inheritdoc />
        public override double Current => ChargeDerivative;

        /// <inheritdoc />
        void ITimeBehavior.InitializeStates()
        {
            if (_time.UseIc && Parameters.InitialCondition.Given)
            {
                _chargeState.Value = EvaluateCharge(Parameters.InitialCondition);
                return;
            }

            _chargeState.Value = EvaluateCharge(Voltage);
        }

        /// <inheritdoc />
        void IBiasingBehavior.Load()
        {
            Load();

            if (_time.UseDc)
            {
                return;
            }

            double voltage = Voltage;
            double charge = EvaluateCharge(voltage);
            double capacitance = EvaluateChargeDerivative(voltage);

            IncrementalCapacitance = capacitance;
            _chargeState.Value = charge;
            _chargeState.Derive();

            JacobianInfo info = _chargeState.GetContributions(capacitance, voltage);
            _timeElements.Add(
                info.Jacobian,
                -info.Jacobian,
                -info.Jacobian,
                info.Jacobian,
                -info.Rhs,
                info.Rhs);
        }
    }
}
