using SpiceSharp;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharpMechanical2D.Bodies;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpMechanical2D.Forces
{
    /// <summary>
    /// Applies a scalar world torque to a planar rigid body.
    /// </summary>
    public sealed class AppliedTorque2D : Entity<AppliedTorque2DParameters>
    {
        /// <summary>Initializes a new applied-torque component.</summary>
        public AppliedTorque2D(string name, string bodyName, double torque)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            Torque = torque;
        }

        /// <summary>Gets the referenced rigid-body entity name.</summary>
        public string BodyName { get; }

        /// <summary>Gets or sets the counterclockwise world torque.</summary>
        public double Torque
        {
            get => Parameters.Torque;
            set => Parameters.Torque = value;
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
            private readonly double[] _values = new double[1];

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                AppliedTorque2DParameters parameters)
                : base(context, body)
            {
                if (!ForceValueValidation.IsFinite(parameters.Torque))
                {
                    throw new SpiceSharpException(
                        $"Applied torque '{Name}' requires a finite torque.");
                }

                _values[0] = parameters.Torque;
                var biasing = context.GetState<IBiasingSimulationState>();
                _elements = new ElementSet<double>(
                    biasing.Solver,
                    new[] { biasing.Map[body.AngularVelocityVariable] });
            }

            protected override void LoadTransient() => _elements.Add(_values);
        }
    }
}
