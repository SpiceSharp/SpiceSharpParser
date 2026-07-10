using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Physics2D.Core
{
    /// <summary>
    /// Transient behavior for <see cref="MechanicalCoordinate"/>.
    /// </summary>
    [GeneratedParameters]
    public sealed partial class MechanicalCoordinateBehavior : Behavior,
        IMechanicalCoordinateBehavior,
        IBiasingBehavior,
        ITimeBehavior,
        IParameterized<MechanicalCoordinateParameters>
    {
        private readonly ElementSet<double> _elements;
        private readonly double[] _elementValues = new double[6];
        private readonly IDerivative _momentumState;
        private readonly IDerivative _positionState;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="MechanicalCoordinateBehavior"/> class.
        /// </summary>
        /// <param name="context">The entity binding context.</param>
        public MechanicalCoordinateBehavior(IBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<MechanicalCoordinateParameters>();
            ValidateParameters();

            var biasing = context.GetState<IBiasingSimulationState>();
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();

            PositionVariable = biasing.CreatePrivateVariable(Name.Combine("position"), Units.Volt);
            VelocityVariable = biasing.CreatePrivateVariable(Name.Combine("velocity"), Units.Volt);

            int position = biasing.Map[PositionVariable];
            int velocity = biasing.Map[VelocityVariable];
            _elements = new ElementSet<double>(
                biasing.Solver,
                new[]
                {
                    new MatrixLocation(position, position),
                    new MatrixLocation(position, velocity),
                    new MatrixLocation(velocity, position),
                    new MatrixLocation(velocity, velocity),
                },
                new[] { position, velocity });

            _positionState = method.CreateDerivative(true);
            _momentumState = method.CreateDerivative(true);
        }

        /// <inheritdoc/>
        public MechanicalCoordinateParameters Parameters { get; }

        /// <inheritdoc/>
        public IVariable<double> PositionVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> VelocityVariable { get; }

        /// <inheritdoc/>
        [ParameterName("position"), ParameterName("q"), ParameterInfo("Generalized position")]
        public double Position => PositionVariable.Value;

        /// <inheritdoc/>
        [ParameterName("velocity"), ParameterName("u"), ParameterInfo("Generalized velocity")]
        public double Velocity => VelocityVariable.Value;

        /// <inheritdoc/>
        [ParameterName("mass"), ParameterName("generalizedmass"), ParameterInfo("Generalized mass")]
        public double GeneralizedMass => Parameters.GeneralizedMass;

        /// <inheritdoc/>
        [ParameterName("initialposition"), ParameterName("q0"), ParameterInfo("Initial generalized position")]
        public double InitialPosition => Parameters.InitialPosition;

        /// <inheritdoc/>
        [ParameterName("initialvelocity"), ParameterName("u0"), ParameterInfo("Initial generalized velocity")]
        public double InitialVelocity => Parameters.InitialVelocity;

        /// <inheritdoc/>
        [ParameterName("kineticenergy"), ParameterName("ke"), ParameterInfo("Kinetic energy")]
        public double KineticEnergy => 0.5 * GeneralizedMass * Velocity * Velocity;

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            _positionState.Value = Position;
            _momentumState.Value = GeneralizedMass * Velocity;
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_time.UseDc)
            {
                LoadInitialState();
                return;
            }

            LoadTransientState();
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);

        private void LoadInitialState()
        {
            _elementValues[0] = 1.0;
            _elementValues[1] = 0.0;
            _elementValues[2] = 0.0;
            _elementValues[3] = 1.0;
            _elementValues[4] = InitialPosition;
            _elementValues[5] = InitialVelocity;
            _elements.Add(_elementValues);
        }

        private void LoadTransientState()
        {
            _positionState.Value = Position;
            _momentumState.Value = GeneralizedMass * Velocity;
            _positionState.Derive();
            _momentumState.Derive();

            JacobianInfo positionDerivative = _positionState.GetContributions(1.0, Position);
            JacobianInfo momentumDerivative = _momentumState.GetContributions(
                GeneralizedMass,
                Velocity);

            // The coordinate owns only qdot - u = 0 and M*udot = 0.
            // Connected force behaviors add -Q and its analytic derivatives
            // directly to the velocity row during their own load pass.
            _elementValues[0] = positionDerivative.Jacobian;
            _elementValues[1] = -1.0;
            _elementValues[2] = 0.0;
            _elementValues[3] = momentumDerivative.Jacobian;
            _elementValues[4] = -positionDerivative.Rhs;
            _elementValues[5] = -momentumDerivative.Rhs;
            _elements.Add(_elementValues);
        }

        private void ValidateParameters()
        {
            if (!(Parameters.GeneralizedMass > 0.0) || !IsFinite(Parameters.GeneralizedMass))
            {
                throw new SpiceSharpException(
                    $"Mechanical coordinate '{Name}' requires a finite generalized mass greater than zero; " +
                    $"received {Parameters.GeneralizedMass:R}.");
            }

            if (!IsFinite(Parameters.InitialPosition))
            {
                throw new SpiceSharpException(
                    $"Mechanical coordinate '{Name}' requires a finite initial position; " +
                    $"received {Parameters.InitialPosition:R}.");
            }

            if (!IsFinite(Parameters.InitialVelocity))
            {
                throw new SpiceSharpException(
                    $"Mechanical coordinate '{Name}' requires a finite initial velocity; " +
                    $"received {Parameters.InitialVelocity:R}.");
            }

            if (Parameters.InitialConditionMode
                != MechanicalInitialConditionMode.HoldSpecifiedStateDuringOperatingPoint)
            {
                throw new SpiceSharpException(
                    $"Mechanical coordinate '{Name}' does not support initial-condition mode " +
                    $"'{Parameters.InitialConditionMode}'.");
            }
        }
    }
}
