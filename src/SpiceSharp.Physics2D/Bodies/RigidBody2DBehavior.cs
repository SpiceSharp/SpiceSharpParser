using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Physics2D.Core;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;

namespace SpiceSharp.Physics2D.Bodies
{
    /// <summary>
    /// Transient behavior for <see cref="RigidBody2D"/>.
    /// </summary>
    [GeneratedParameters]
    public sealed partial class RigidBody2DBehavior : Behavior,
        IRigidBody2DBehavior,
        IBiasingBehavior,
        ITimeBehavior,
        IParameterized<RigidBody2DParameters>
    {
        private const int MatrixValueCount = 12;
        private readonly ElementSet<double> _elements;
        private readonly double[] _elementValues = new double[18];
        private readonly IDerivative[] _momentumStates;
        private readonly IDerivative[] _positionStates;
        private readonly IVariable<double>[] _positionVariables;
        private readonly ITimeSimulationState _time;
        private readonly IVariable<double>[] _velocityVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="RigidBody2DBehavior"/> class.
        /// </summary>
        /// <param name="context">The entity binding context.</param>
        public RigidBody2DBehavior(IBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<RigidBody2DParameters>();
            ValidateParameters();

            var biasing = context.GetState<IBiasingSimulationState>();
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();

            PositionXVariable = biasing.CreatePrivateVariable(Name.Combine("positionx"), Units.Volt);
            PositionYVariable = biasing.CreatePrivateVariable(Name.Combine("positiony"), Units.Volt);
            AngleVariable = biasing.CreatePrivateVariable(Name.Combine("angle"), Units.Volt);
            VelocityXVariable = biasing.CreatePrivateVariable(Name.Combine("velocityx"), Units.Volt);
            VelocityYVariable = biasing.CreatePrivateVariable(Name.Combine("velocityy"), Units.Volt);
            AngularVelocityVariable = biasing.CreatePrivateVariable(
                Name.Combine("angularvelocity"),
                Units.Volt);

            _positionVariables = new[]
            {
                PositionXVariable,
                PositionYVariable,
                AngleVariable,
            };
            _velocityVariables = new[]
            {
                VelocityXVariable,
                VelocityYVariable,
                AngularVelocityVariable,
            };
            _positionStates = new[]
            {
                method.CreateDerivative(true),
                method.CreateDerivative(true),
                method.CreateDerivative(true),
            };
            _momentumStates = new[]
            {
                method.CreateDerivative(true),
                method.CreateDerivative(true),
                method.CreateDerivative(true),
            };

            int positionX = biasing.Map[PositionXVariable];
            int positionY = biasing.Map[PositionYVariable];
            int angle = biasing.Map[AngleVariable];
            int velocityX = biasing.Map[VelocityXVariable];
            int velocityY = biasing.Map[VelocityYVariable];
            int angularVelocity = biasing.Map[AngularVelocityVariable];
            _elements = new ElementSet<double>(
                biasing.Solver,
                new[]
                {
                    new MatrixLocation(positionX, positionX),
                    new MatrixLocation(positionX, velocityX),
                    new MatrixLocation(velocityX, positionX),
                    new MatrixLocation(velocityX, velocityX),
                    new MatrixLocation(positionY, positionY),
                    new MatrixLocation(positionY, velocityY),
                    new MatrixLocation(velocityY, positionY),
                    new MatrixLocation(velocityY, velocityY),
                    new MatrixLocation(angle, angle),
                    new MatrixLocation(angle, angularVelocity),
                    new MatrixLocation(angularVelocity, angle),
                    new MatrixLocation(angularVelocity, angularVelocity),
                },
                new[]
                {
                    positionX,
                    velocityX,
                    positionY,
                    velocityY,
                    angle,
                    angularVelocity,
                });
        }

        /// <inheritdoc/>
        public RigidBody2DParameters Parameters { get; }

        /// <inheritdoc/>
        public IVariable<double> PositionXVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> PositionYVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> AngleVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> VelocityXVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> VelocityYVariable { get; }

        /// <inheritdoc/>
        public IVariable<double> AngularVelocityVariable { get; }

        /// <inheritdoc/>
        [ParameterName("positionx"), ParameterName("x"), ParameterInfo("World x-position")]
        public double PositionX => PositionXVariable.Value;

        /// <inheritdoc/>
        [ParameterName("positiony"), ParameterName("y"), ParameterInfo("World y-position")]
        public double PositionY => PositionYVariable.Value;

        /// <inheritdoc/>
        [ParameterName("angle"), ParameterName("theta"), ParameterInfo("Unbounded angle")]
        public double Angle => AngleVariable.Value;

        /// <inheritdoc/>
        [ParameterName("velocityx"), ParameterName("vx"), ParameterInfo("World x-velocity")]
        public double VelocityX => VelocityXVariable.Value;

        /// <inheritdoc/>
        [ParameterName("velocityy"), ParameterName("vy"), ParameterInfo("World y-velocity")]
        public double VelocityY => VelocityYVariable.Value;

        /// <inheritdoc/>
        [ParameterName("angularvelocity"), ParameterName("omega"), ParameterInfo("Angular velocity")]
        public double AngularVelocity => AngularVelocityVariable.Value;

        /// <inheritdoc/>
        public Vector2D Position => new Vector2D(PositionX, PositionY);

        /// <inheritdoc/>
        public Vector2D LinearVelocity => new Vector2D(VelocityX, VelocityY);

        /// <inheritdoc/>
        [ParameterName("mass"), ParameterInfo("Translational mass")]
        public double Mass => Parameters.Mass;

        /// <inheritdoc/>
        [ParameterName("inertia"), ParameterInfo("Rotational inertia")]
        public double Inertia => Parameters.Inertia;

        /// <inheritdoc/>
        [ParameterName("linearkineticenergy"), ParameterName("linearenergy"), ParameterInfo("Linear kinetic energy")]
        public double LinearKineticEnergy =>
            0.5 * Mass * ((VelocityX * VelocityX) + (VelocityY * VelocityY));

        /// <inheritdoc/>
        [ParameterName("angularkineticenergy"), ParameterName("angularenergy"), ParameterInfo("Angular kinetic energy")]
        public double AngularKineticEnergy =>
            0.5 * Inertia * AngularVelocity * AngularVelocity;

        /// <inheritdoc/>
        [ParameterName("kineticenergy"), ParameterName("ke"), ParameterInfo("Total kinetic energy")]
        public double KineticEnergy => LinearKineticEnergy + AngularKineticEnergy;

        /// <inheritdoc/>
        public Vector2D LocalPointToWorld(Vector2D localPoint) =>
            Position + LocalVectorToWorld(localPoint);

        /// <inheritdoc/>
        public Vector2D LocalVectorToWorld(Vector2D localVector) => localVector.Rotate(Angle);

        /// <inheritdoc/>
        public Vector2D WorldPointToLocal(Vector2D worldPoint) =>
            (worldPoint - Position).Rotate(-Angle);

        /// <inheritdoc/>
        public Vector2D WorldVectorToLocal(Vector2D worldVector) => worldVector.Rotate(-Angle);

        /// <inheritdoc/>
        public Vector2D GetPointVelocity(Vector2D localPoint)
        {
            Vector2D worldOffset = LocalVectorToWorld(localPoint);
            return LinearVelocity + (AngularVelocity * worldOffset.Perpendicular());
        }

        /// <inheritdoc/>
        public double ComputeTorque(Vector2D localPoint, Vector2D worldForce) =>
            Vector2D.Cross(LocalVectorToWorld(localPoint), worldForce);

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            for (int index = 0; index < _positionStates.Length; index++)
            {
                double generalizedMass = GetGeneralizedMass(index);
                _positionStates[index].Value = _positionVariables[index].Value;
                _momentumStates[index].Value = generalizedMass * _velocityVariables[index].Value;
            }
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

        private double GetGeneralizedMass(int index) => index < 2 ? Mass : Inertia;

        private double GetInitialPosition(int index)
        {
            switch (index)
            {
                case 0:
                    return Parameters.InitialPositionX;
                case 1:
                    return Parameters.InitialPositionY;
                default:
                    return Parameters.InitialAngle;
            }
        }

        private double GetInitialVelocity(int index)
        {
            switch (index)
            {
                case 0:
                    return Parameters.InitialVelocityX;
                case 1:
                    return Parameters.InitialVelocityY;
                default:
                    return Parameters.InitialAngularVelocity;
            }
        }

        private void LoadInitialState()
        {
            for (int index = 0; index < 3; index++)
            {
                int matrixOffset = index * 4;
                int rhsOffset = MatrixValueCount + (index * 2);
                _elementValues[matrixOffset] = 1.0;
                _elementValues[matrixOffset + 1] = 0.0;
                _elementValues[matrixOffset + 2] = 0.0;
                _elementValues[matrixOffset + 3] = 1.0;
                _elementValues[rhsOffset] = GetInitialPosition(index);
                _elementValues[rhsOffset + 1] = GetInitialVelocity(index);
            }

            _elements.Add(_elementValues);
        }

        private void LoadTransientState()
        {
            for (int index = 0; index < 3; index++)
            {
                double position = _positionVariables[index].Value;
                double velocity = _velocityVariables[index].Value;
                double generalizedMass = GetGeneralizedMass(index);
                IDerivative positionState = _positionStates[index];
                IDerivative momentumState = _momentumStates[index];

                positionState.Value = position;
                momentumState.Value = generalizedMass * velocity;
                positionState.Derive();
                momentumState.Derive();

                JacobianInfo positionDerivative = positionState.GetContributions(1.0, position);
                JacobianInfo momentumDerivative = momentumState.GetContributions(
                    generalizedMass,
                    velocity);
                int matrixOffset = index * 4;
                int rhsOffset = MatrixValueCount + (index * 2);
                _elementValues[matrixOffset] = positionDerivative.Jacobian;
                _elementValues[matrixOffset + 1] = -1.0;
                _elementValues[matrixOffset + 2] = 0.0;
                _elementValues[matrixOffset + 3] = momentumDerivative.Jacobian;
                _elementValues[rhsOffset] = -positionDerivative.Rhs;
                _elementValues[rhsOffset + 1] = -momentumDerivative.Rhs;
            }

            _elements.Add(_elementValues);
        }

        private void ValidateParameters()
        {
            if (!(Parameters.Mass > 0.0) || !IsFinite(Parameters.Mass))
            {
                throw new SpiceSharpException(
                    $"Rigid body '{Name}' requires a finite mass greater than zero; " +
                    $"received {Parameters.Mass:R}.");
            }

            if (!(Parameters.Inertia > 0.0) || !IsFinite(Parameters.Inertia))
            {
                throw new SpiceSharpException(
                    $"Rigid body '{Name}' requires a finite inertia greater than zero; " +
                    $"received {Parameters.Inertia:R}.");
            }

            ValidateFiniteInitialValue(Parameters.InitialPositionX, "initial x-position");
            ValidateFiniteInitialValue(Parameters.InitialPositionY, "initial y-position");
            ValidateFiniteInitialValue(Parameters.InitialAngle, "initial angle");
            ValidateFiniteInitialValue(Parameters.InitialVelocityX, "initial x-velocity");
            ValidateFiniteInitialValue(Parameters.InitialVelocityY, "initial y-velocity");
            ValidateFiniteInitialValue(
                Parameters.InitialAngularVelocity,
                "initial angular velocity");

            if (Parameters.InitialConditionMode
                != MechanicalInitialConditionMode.HoldSpecifiedStateDuringOperatingPoint)
            {
                throw new SpiceSharpException(
                    $"Rigid body '{Name}' does not support initial-condition mode " +
                    $"'{Parameters.InitialConditionMode}'.");
            }
        }

        private void ValidateFiniteInitialValue(double value, string description)
        {
            if (!IsFinite(value))
            {
                throw new SpiceSharpException(
                    $"Rigid body '{Name}' requires a finite {description}; received {value:R}.");
            }
        }
    }
}
