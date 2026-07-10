using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharpMechanical2D.Mathematics;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Applies a force at a body-local point.
    /// </summary>
    public sealed class PointForce2D : Entity<PointForce2DParameters>
    {
        /// <summary>Initializes a new point-force component.</summary>
        public PointForce2D(
            string name,
            string bodyName,
            Vector2D localPoint,
            Vector2D force,
            ForceCoordinateSystem2D forceCoordinates = ForceCoordinateSystem2D.World)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            LocalPoint = localPoint;
            Force = force;
            ForceCoordinates = forceCoordinates;
        }

        /// <summary>Gets the referenced rigid-body entity name.</summary>
        public string BodyName { get; }

        /// <summary>Gets or sets the body-local force application point.</summary>
        public Vector2D LocalPoint
        {
            get => new Vector2D(Parameters.LocalPointX, Parameters.LocalPointY);
            set
            {
                Parameters.LocalPointX = value.X;
                Parameters.LocalPointY = value.Y;
            }
        }

        /// <summary>Gets or sets the force in <see cref="ForceCoordinates"/>.</summary>
        public Vector2D Force
        {
            get => new Vector2D(Parameters.ForceX, Parameters.ForceY);
            set
            {
                Parameters.ForceX = value.X;
                Parameters.ForceY = value.Y;
            }
        }

        /// <summary>Gets or sets the coordinate system of <see cref="Force"/>.</summary>
        public ForceCoordinateSystem2D ForceCoordinates
        {
            get => Parameters.ForceCoordinates;
            set => Parameters.ForceCoordinates = value;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!simulation.UsesBehaviors<ITimeBehavior>())
            {
                return;
            }

            IRigidBody2DBehavior body = RigidBodyLoadBinding.ResolveBody(BodyName, simulation);
            var behaviors = new BehaviorContainer(Name);
            var context = new BindingContext(this, simulation, behaviors);
            behaviors.Add(new LoadBehavior(context, body, Parameters));
            simulation.EntityBehaviors.Add(behaviors);
        }

        private sealed class LoadBehavior : RigidBodyLoadBehavior
        {
            private readonly ElementSet<double> _elements;
            private readonly PointForce2DParameters _parameters;
            private readonly double[] _values = new double[6];

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                PointForce2DParameters parameters)
                : base(context, body)
            {
                _parameters = parameters;
                if (!ForceValueValidation.IsFinite(LocalPoint))
                {
                    throw new SpiceSharpException(
                        $"Point force '{Name}' requires a finite body-local point.");
                }

                if (!ForceValueValidation.IsFinite(Force))
                {
                    throw new SpiceSharpException(
                        $"Point force '{Name}' requires a finite force vector.");
                }

                if (_parameters.ForceCoordinates != ForceCoordinateSystem2D.World
                    && _parameters.ForceCoordinates != ForceCoordinateSystem2D.BodyLocal)
                {
                    throw new SpiceSharpException(
                        $"Point force '{Name}' does not support force coordinates " +
                        $"'{_parameters.ForceCoordinates}'.");
                }

                var biasing = context.GetState<IBiasingSimulationState>();
                int angle = biasing.Map[body.AngleVariable];
                int velocityX = biasing.Map[body.VelocityXVariable];
                int velocityY = biasing.Map[body.VelocityYVariable];
                int angularVelocity = biasing.Map[body.AngularVelocityVariable];
                _elements = new ElementSet<double>(
                    biasing.Solver,
                    new[]
                    {
                        new MatrixLocation(velocityX, angle),
                        new MatrixLocation(velocityY, angle),
                        new MatrixLocation(angularVelocity, angle),
                    },
                    new[] { velocityX, velocityY, angularVelocity });
            }

            private Vector2D LocalPoint =>
                new Vector2D(_parameters.LocalPointX, _parameters.LocalPointY);

            private Vector2D Force => new Vector2D(_parameters.ForceX, _parameters.ForceY);

            protected override void LoadTransient()
            {
                double angle = Body.Angle;
                PointForce2DContribution contribution = PointForce2DEquation.Evaluate(
                    angle,
                    LocalPoint,
                    Force,
                    _parameters.ForceCoordinates);
                Vector2D forceDerivative = contribution.WorldForceDerivativeByAngle;

                // The body residuals contain -Fx, -Fy, and -tau. Their Newton
                // matrices therefore receive the negative load derivatives.
                _values[0] = -forceDerivative.X;
                _values[1] = -forceDerivative.Y;
                _values[2] = -contribution.TorqueDerivativeByAngle;
                _values[3] = contribution.WorldForce.X - (forceDerivative.X * angle);
                _values[4] = contribution.WorldForce.Y - (forceDerivative.Y * angle);
                _values[5] = contribution.Torque
                    - (contribution.TorqueDerivativeByAngle * angle);
                _elements.Add(_values);
            }
        }
    }
}
