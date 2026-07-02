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

            // _chargeState represents i = dQ/dt for this candidate timestep.
            // MNA cannot stamp a time derivative directly. GetContributions()
            // converts the derivative and Newton tangent into a branch-current
            // companion form:
            //
            //   iC ~= J * Vcap + Ieq
            //   Vcap = Vpos - Vneg
            //
            // For backward Euler, dQ/dt ~= (Qnow - Qprev) / h. The current charge
            // term Qnow depends on the unknown Vcap, while Qprev is accepted
            // history:
            //
            //   i = dQ/dt ~= (Qnow - Qprev) / h
            //              = (1/h) * Q(Vcap) + (-(1/h) * Qprev)
            //
            // In the common integration-method notation:
            //
            //   i ~= a0 * Q(Vcap) + history
            //   a0 = 1/h
            //   history = -(1/h) * Qprev
            //
            // If Q(Vcap) is nonlinear, Newton replaces it by its local tangent
            // at the current voltage guess:
            //
            //   Q(Vcap) ~= Q(Vguess) + Cinc * (Vcap - Vguess)
            //
            // Substituting that tangent into the integration formula gives:
            //
            //   iC ~= (a0*Cinc) * Vcap
            //       + history
            //       + a0 * (Q(Vguess) - Cinc*Vguess)
            //
            // So GetContributions returns the two pieces used by MNA:
            //
            //   J   ~= a0*Cinc
            //   Ieq ~= history + a0*(Q(Vguess) - Cinc*Vguess)
            //
            // In MNA, the branch current leaves C+ and enters C-. KCL therefore
            // adds +iC to the C+ row and -iC to the C- row:
            //
            //   C+ row: +J*Vpos - J*Vneg = -Ieq
            //   C- row: -J*Vpos + J*Vneg = +Ieq
            //
            // ElementSet order:
            // Y[pos,pos], Y[pos,neg], Y[neg,pos], Y[neg,neg], rhs[pos], rhs[neg].
            _timeElements.Add(
                info.Jacobian, // +J*Vpos in the positive-node KCL row.
                -info.Jacobian, // -J*Vneg in the positive-node KCL row.
                -info.Jacobian, // -J*Vpos in the negative-node KCL row.
                info.Jacobian, // +J*Vneg in the negative-node KCL row.
                -info.Rhs, // -Ieq in the positive-node RHS.
                info.Rhs); // +Ieq in the negative-node RHS.
        }
    }
}
