using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.Entities;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    /// <summary>
    /// A behavior with the sole purpose of providing a parameter set.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{P}" />
    [BehaviorFor(typeof(ISwitchModel), typeof(ITemperatureBehavior))]
    public class ISwitchModelBehavior : ParameterBehavior<ISwitchModelParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ISwitchModelBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ISwitchModelBehavior(IBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<ISwitchModelParameters>();
        }

        public new ISwitchModelParameters Parameters { get; }
    }
}
