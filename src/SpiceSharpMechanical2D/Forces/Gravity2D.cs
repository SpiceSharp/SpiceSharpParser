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
    /// Applies a constant world-frame gravitational acceleration to a rigid body.
    /// </summary>
    public sealed class Gravity2D : Entity<Gravity2DParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gravity2D"/> class.
        /// </summary>
        public Gravity2D(string name, string bodyName, Vector2D acceleration)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            Acceleration = acceleration;
        }

        /// <summary>Gets the referenced rigid-body entity name.</summary>
        public string BodyName { get; }

        /// <summary>Gets or sets the world-frame gravitational acceleration.</summary>
        public Vector2D Acceleration
        {
            get => new Vector2D(Parameters.AccelerationX, Parameters.AccelerationY);
            set
            {
                Parameters.AccelerationX = value.X;
                Parameters.AccelerationY = value.Y;
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
            private readonly Gravity2DParameters _parameters;
            private readonly ElementSet<double> _elements;
            private readonly double[] _values = new double[2];

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                Gravity2DParameters parameters)
                : base(context, body)
            {
                _parameters = parameters;
                if (!ForceValueValidation.IsFinite(Acceleration))
                {
                    throw new SpiceSharpException(
                        $"Gravity '{Name}' requires a finite world acceleration.");
                }

                var biasing = context.GetState<IBiasingSimulationState>();
                _elements = new ElementSet<double>(
                    biasing.Solver,
                    new[]
                    {
                        biasing.Map[body.VelocityXVariable],
                        biasing.Map[body.VelocityYVariable],
                    });
            }

            private Vector2D Acceleration =>
                new Vector2D(_parameters.AccelerationX, _parameters.AccelerationY);

            protected override void LoadTransient()
            {
                Vector2D acceleration = Acceleration;
                if (!ForceValueValidation.IsFinite(acceleration))
                {
                    throw new SpiceSharpException(
                        $"Gravity '{Name}' evaluated a non-finite world acceleration.");
                }

                _values[0] = Body.Mass * acceleration.X;
                _values[1] = Body.Mass * acceleration.Y;
                _elements.Add(_values);
            }
        }
    }
}
