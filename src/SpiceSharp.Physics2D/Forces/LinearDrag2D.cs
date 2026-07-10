using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Physics2D.Forces
{
    /// <summary>
    /// Applies isotropic linear drag relative to a world-frame medium velocity.
    /// </summary>
    public sealed class LinearDrag2D : Entity<LinearDrag2DParameters>
    {
        /// <summary>Initializes a new linear-drag component.</summary>
        public LinearDrag2D(
            string name,
            string bodyName,
            double damping,
            Vector2D mediumVelocity = default)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            Damping = damping;
            MediumVelocity = mediumVelocity;
        }

        /// <summary>Gets the referenced rigid-body entity name.</summary>
        public string BodyName { get; }

        /// <summary>Gets or sets the nonnegative linear damping coefficient.</summary>
        public double Damping
        {
            get => Parameters.Damping;
            set => Parameters.Damping = value;
        }

        /// <summary>Gets or sets the world-frame velocity of the surrounding medium.</summary>
        public Vector2D MediumVelocity
        {
            get => new Vector2D(Parameters.MediumVelocityX, Parameters.MediumVelocityY);
            set
            {
                Parameters.MediumVelocityX = value.X;
                Parameters.MediumVelocityY = value.Y;
            }
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
            private readonly double[] _values = new double[4];

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                LinearDrag2DParameters parameters)
                : base(context, body)
            {
                if (parameters.Damping < 0.0
                    || !ForceValueValidation.IsFinite(parameters.Damping))
                {
                    throw new SpiceSharpException(
                        $"Linear drag '{Name}' requires finite nonnegative damping.");
                }

                var mediumVelocity = new Vector2D(
                    parameters.MediumVelocityX,
                    parameters.MediumVelocityY);
                if (!ForceValueValidation.IsFinite(mediumVelocity))
                {
                    throw new SpiceSharpException(
                        $"Linear drag '{Name}' requires a finite medium velocity.");
                }

                _values[0] = parameters.Damping;
                _values[1] = parameters.Damping;
                _values[2] = parameters.Damping * mediumVelocity.X;
                _values[3] = parameters.Damping * mediumVelocity.Y;
                var biasing = context.GetState<IBiasingSimulationState>();
                int velocityX = biasing.Map[body.VelocityXVariable];
                int velocityY = biasing.Map[body.VelocityYVariable];
                _elements = new ElementSet<double>(
                    biasing.Solver,
                    new[]
                    {
                        new MatrixLocation(velocityX, velocityX),
                        new MatrixLocation(velocityY, velocityY),
                    },
                    new[] { velocityX, velocityY });
            }

            protected override void LoadTransient() => _elements.Add(_values);
        }
    }
}
