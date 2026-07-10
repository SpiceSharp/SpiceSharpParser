using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharp.Physics2D.ApiProbe
{
    /// <summary>
    /// Proves the public SpiceSharp extension APIs needed by a two-state transient entity.
    /// </summary>
    /// <remarks>
    /// The probe solves <c>dA/dt = B</c> and <c>dB/dt = -A</c>. It is an API proof,
    /// not a mechanical model.
    /// </remarks>
    public sealed class TransientApiProbe : Entity<TransientApiProbeParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransientApiProbe"/> class.
        /// </summary>
        /// <param name="name">Entity name.</param>
        /// <param name="initialA">Initial value of state A.</param>
        /// <param name="initialB">Initial value of state B.</param>
        /// <param name="linkedProbeName">Optional name of another probe to resolve during setup.</param>
        public TransientApiProbe(
            string name,
            double initialA = 1.0,
            double initialB = 0.0,
            string linkedProbeName = null)
            : base(name)
        {
            Parameters.InitialA = initialA;
            Parameters.InitialB = initialB;
            Parameters.LinkedProbeName = linkedProbeName;
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

            ITransientApiProbeBehavior linkedBehavior = null;
            if (!string.IsNullOrWhiteSpace(Parameters.LinkedProbeName))
            {
                var reference = new Reference(Parameters.LinkedProbeName);
                var linkedBehaviors = reference.GetContainer(simulation);
                if (!linkedBehaviors.TryGetValue(out linkedBehavior))
                {
                    throw new SpiceSharpException(
                        $"Entity '{Name}' could not resolve an {nameof(ITransientApiProbeBehavior)} " +
                        $"from linked entity '{Parameters.LinkedProbeName}'.");
                }
            }

            var behaviors = new BehaviorContainer(Name);
            var context = new BindingContext(this, simulation, behaviors);
            behaviors.Add(new TransientApiProbeBehavior(context, linkedBehavior));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
