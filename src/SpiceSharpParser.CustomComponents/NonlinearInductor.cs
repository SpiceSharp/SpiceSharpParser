using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.NonlinearInductors;
using System;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// A standalone LTspice-style nonlinear inductor driven by a Flux(x) expression.
    /// </summary>
    [Pin(0, "L+"), Pin(1, "L-")]
    public class NonlinearInductor : Component<NonlinearInductorParameters>
    {
        /// <summary>
        /// The pin count for nonlinear inductors.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int PinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearInductor"/> class.
        /// </summary>
        /// <param name="name">The device name.</param>
        public NonlinearInductor(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearInductor"/> class.
        /// </summary>
        /// <param name="name">The device name.</param>
        /// <param name="positive">The positive node.</param>
        /// <param name="negative">The negative node.</param>
        /// <param name="fluxExpression">The LTspice-style flux expression.</param>
        public NonlinearInductor(string name, string positive, string negative, string fluxExpression)
            : this(name)
        {
            Connect(positive, negative);
            Parameters.Expression = fluxExpression;
        }

        /// <inheritdoc />
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors);

            if (simulation is IFrequencySimulation || simulation.UsesBehaviors<IFrequencyBehavior>())
            {
                behaviors.Add(new Frequency(context));
            }
            else if (simulation.UsesBehaviors<ITimeBehavior>())
            {
                behaviors.Add(new Time(context));
            }
            else if (simulation.UsesBehaviors<IBiasingBehavior>())
            {
                behaviors.Add(new Biasing(context));
            }

            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
