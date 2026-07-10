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
    /// Applies a world-frame force through a rigid body's center of mass.
    /// </summary>
    public sealed class AppliedForce2D : Entity<AppliedForce2DParameters>
    {
        /// <summary>Creates a constant world-force component.</summary>
        public AppliedForce2D(string name, string bodyName, Vector2D worldForce)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            WorldForce = worldForce;
        }

        /// <summary>Creates a deterministic time-dependent world-force component.</summary>
        public AppliedForce2D(
            string name,
            string bodyName,
            ForceFunction2D forceFunction)
            : base(name)
        {
            BodyName = bodyName ?? throw new ArgumentNullException(nameof(bodyName));
            ForceFunction = forceFunction ?? throw new ArgumentNullException(nameof(forceFunction));
        }

        /// <summary>Gets the referenced rigid-body entity name.</summary>
        public string BodyName { get; }

        /// <summary>
        /// Gets or sets the constant world force used when no force function is set.
        /// </summary>
        public Vector2D WorldForce
        {
            get => new Vector2D(Parameters.ForceX, Parameters.ForceY);
            set
            {
                Parameters.ForceX = value.X;
                Parameters.ForceY = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the optional deterministic transient force function.
        /// </summary>
        /// <remarks>When non-null, this function takes precedence over <see cref="WorldForce"/>.</remarks>
        public ForceFunction2D ForceFunction
        {
            get => Parameters.ForceFunction;
            set => Parameters.ForceFunction = value;
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
            private readonly IIntegrationMethod _method;
            private readonly AppliedForce2DParameters _parameters;
            private readonly double[] _values = new double[2];

            public LoadBehavior(
                IBindingContext context,
                IRigidBody2DBehavior body,
                AppliedForce2DParameters parameters)
                : base(context, body)
            {
                _parameters = parameters;
                _method = context.GetState<IIntegrationMethod>();
                if (_parameters.ForceFunction == null
                    && !ForceValueValidation.IsFinite(ConstantForce))
                {
                    throw new SpiceSharpException(
                        $"Applied force '{Name}' requires a finite world force.");
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

            private Vector2D ConstantForce =>
                new Vector2D(_parameters.ForceX, _parameters.ForceY);

            protected override void LoadTransient()
            {
                Vector2D force = _parameters.ForceFunction != null
                    ? _parameters.ForceFunction(_method.Time)
                    : ConstantForce;
                if (!ForceValueValidation.IsFinite(force))
                {
                    throw new SpiceSharpException(
                        $"Applied force '{Name}' evaluated a non-finite force at " +
                        $"time {_method.Time:R}.");
                }

                _values[0] = force.X;
                _values[1] = force.Y;
                _elements.Add(_values);
            }
        }
    }
}
