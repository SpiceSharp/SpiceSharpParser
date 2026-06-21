using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents.NonlinearCapacitors;
using System;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// A standalone LTspice-style nonlinear capacitor driven by a Q(x) expression.
    /// </summary>
    [Pin(0, "C+"), Pin(1, "C-")]
    public class NonlinearCapacitor : Component<NonlinearCapacitorParameters>
    {
        /// <summary>
        /// The pin count for nonlinear capacitors.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int PinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearCapacitor"/> class.
        /// </summary>
        /// <param name="name">The device name.</param>
        public NonlinearCapacitor(string name)
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonlinearCapacitor"/> class.
        /// </summary>
        /// <param name="name">The device name.</param>
        /// <param name="positive">The positive node.</param>
        /// <param name="negative">The negative node.</param>
        /// <param name="chargeExpression">The LTspice-style charge expression.</param>
        public NonlinearCapacitor(string name, string positive, string negative, string chargeExpression)
            : this(name)
        {
            Connect(positive, negative);
            Parameters.Expression = chargeExpression;
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
