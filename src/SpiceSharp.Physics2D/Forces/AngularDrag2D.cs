using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Physics2D.Forces
{
    /// <summary>
    /// Applies angular drag relative to a surrounding-medium angular velocity.
    /// </summary>
    public sealed class AngularDrag2D : Entity<AngularDrag2DParameters>
    {
        /// <summary>Initializes a new angular-drag component.</summary>
        public AngularDrag2D(
            string name,
            string bodyName,
            double damping,
            double mediumAngularVelocity = 0.0)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            Damping = damping;
            MediumAngularVelocity = mediumAngularVelocity;
        }

        /// <summary>Gets the referenced rigid-body entity name.</summary>
        public string BodyName { get; }

        /// <summary>Gets or sets the nonnegative angular damping coefficient.</summary>
        public double Damping
        {
            get => Parameters.Damping;
            set => Parameters.Damping = value;
        }

        /// <summary>Gets or sets the surrounding-medium angular velocity.</summary>
        public double MediumAngularVelocity
        {
            get => Parameters.MediumAngularVelocity;
            set => Parameters.MediumAngularVelocity = value;
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
            private readonly double[] _values = new double[2];

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                AngularDrag2DParameters parameters)
                : base(context, body)
            {
                if (parameters.Damping < 0.0
                    || !ForceValueValidation.IsFinite(parameters.Damping))
                {
                    throw new SpiceSharpException(
                        $"Angular drag '{Name}' requires finite nonnegative damping.");
                }

                if (!ForceValueValidation.IsFinite(parameters.MediumAngularVelocity))
                {
                    throw new SpiceSharpException(
                        $"Angular drag '{Name}' requires a finite medium angular velocity.");
                }

                _values[0] = parameters.Damping;
                _values[1] = parameters.Damping * parameters.MediumAngularVelocity;
                var biasing = context.GetState<IBiasingSimulationState>();
                int angularVelocity = biasing.Map[body.AngularVelocityVariable];
                _elements = new ElementSet<double>(
                    biasing.Solver,
                    new[] { new MatrixLocation(angularVelocity, angularVelocity) },
                    new[] { angularVelocity });
            }

            protected override void LoadTransient() => _elements.Add(_values);
        }
    }
}
