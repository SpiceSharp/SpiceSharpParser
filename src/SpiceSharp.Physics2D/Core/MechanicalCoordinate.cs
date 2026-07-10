using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;

namespace SpiceSharp.Physics2D.Core
{
    /// <summary>
    /// Represents one generalized mechanical coordinate integrated by an
    /// ordinary SpiceSharp transient simulation.
    /// </summary>
    public sealed class MechanicalCoordinate : Entity<MechanicalCoordinateParameters>, IRuleSubject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MechanicalCoordinate"/> class.
        /// </summary>
        /// <param name="name">The entity name.</param>
        /// <param name="generalizedMass">The generalized mass or rotational inertia.</param>
        /// <param name="initialPosition">The initial generalized position.</param>
        /// <param name="initialVelocity">The initial generalized velocity.</param>
        public MechanicalCoordinate(
            string name,
            double generalizedMass,
            double initialPosition = 0.0,
            double initialVelocity = 0.0)
            : base(name)
        {
            Parameters.GeneralizedMass = generalizedMass;
            Parameters.InitialPosition = initialPosition;
            Parameters.InitialVelocity = initialVelocity;
        }

        /// <summary>
        /// Gets or sets the generalized mass or rotational inertia.
        /// </summary>
        public double GeneralizedMass
        {
            get => Parameters.GeneralizedMass;
            set => Parameters.GeneralizedMass = value;
        }

        /// <summary>
        /// Gets or sets the requested initial generalized position.
        /// </summary>
        public double InitialPosition
        {
            get => Parameters.InitialPosition;
            set => Parameters.InitialPosition = value;
        }

        /// <summary>
        /// Gets or sets the requested initial generalized velocity.
        /// </summary>
        public double InitialVelocity
        {
            get => Parameters.InitialVelocity;
            set => Parameters.InitialVelocity = value;
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
            behaviors.Add(new MechanicalCoordinateBehavior(context));
            simulation.EntityBehaviors.Add(behaviors);
        }

        void IRuleSubject.Apply(IRules rules) =>
            MechanicalValidation.RegisterGroundReference(this, rules);
    }
}
