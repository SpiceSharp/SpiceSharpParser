using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Physics2D.Core;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Physics2D.Bodies
{
    /// <summary>
    /// Represents a planar rigid body integrated by an ordinary SpiceSharp
    /// transient simulation.
    /// </summary>
    public sealed class RigidBody2D : Entity<RigidBody2DParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RigidBody2D"/> class.
        /// </summary>
        /// <param name="name">The entity name.</param>
        /// <param name="mass">The translational mass.</param>
        /// <param name="inertia">The rotational moment of inertia about the center of mass.</param>
        /// <param name="initialPosition">The initial world position of the center of mass.</param>
        /// <param name="initialAngle">The initial unbounded counterclockwise angle in radians.</param>
        /// <param name="initialLinearVelocity">The initial world linear velocity.</param>
        /// <param name="initialAngularVelocity">The initial counterclockwise angular velocity.</param>
        public RigidBody2D(
            string name,
            double mass,
            double inertia,
            Vector2D initialPosition = default,
            double initialAngle = 0.0,
            Vector2D initialLinearVelocity = default,
            double initialAngularVelocity = 0.0)
            : base(name)
        {
            Parameters.Mass = mass;
            Parameters.Inertia = inertia;
            Parameters.InitialPositionX = initialPosition.X;
            Parameters.InitialPositionY = initialPosition.Y;
            Parameters.InitialAngle = initialAngle;
            Parameters.InitialVelocityX = initialLinearVelocity.X;
            Parameters.InitialVelocityY = initialLinearVelocity.Y;
            Parameters.InitialAngularVelocity = initialAngularVelocity;
        }

        /// <summary>
        /// Gets or sets the translational mass.
        /// </summary>
        public double Mass
        {
            get => Parameters.Mass;
            set => Parameters.Mass = value;
        }

        /// <summary>
        /// Gets or sets the rotational moment of inertia about the center of mass.
        /// </summary>
        public double Inertia
        {
            get => Parameters.Inertia;
            set => Parameters.Inertia = value;
        }

        /// <summary>
        /// Gets or sets the requested initial world position.
        /// </summary>
        public Vector2D InitialPosition
        {
            get => new Vector2D(Parameters.InitialPositionX, Parameters.InitialPositionY);
            set
            {
                Parameters.InitialPositionX = value.X;
                Parameters.InitialPositionY = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the requested initial unbounded angle in radians.
        /// </summary>
        public double InitialAngle
        {
            get => Parameters.InitialAngle;
            set => Parameters.InitialAngle = value;
        }

        /// <summary>
        /// Gets or sets the requested initial world linear velocity.
        /// </summary>
        public Vector2D InitialLinearVelocity
        {
            get => new Vector2D(Parameters.InitialVelocityX, Parameters.InitialVelocityY);
            set
            {
                Parameters.InitialVelocityX = value.X;
                Parameters.InitialVelocityY = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the requested initial counterclockwise angular velocity.
        /// </summary>
        public double InitialAngularVelocity
        {
            get => Parameters.InitialAngularVelocity;
            set => Parameters.InitialAngularVelocity = value;
        }

        /// <summary>
        /// Gets or sets the initial-condition policy.
        /// </summary>
        public MechanicalInitialConditionMode InitialConditionMode
        {
            get => Parameters.InitialConditionMode;
            set => Parameters.InitialConditionMode = value;
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

            var behaviors = new BehaviorContainer(Name);
            var context = new BindingContext(this, simulation, behaviors);
            behaviors.Add(new RigidBody2DBehavior(context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
