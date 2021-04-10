using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Common;
using SpiceSharp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    /// <summary>
    /// A behavior with the sole purpose of providing a parameter set.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{P}" />
    [BehaviorFor(typeof(VSwitchModel), typeof(ITemperatureBehavior))]
    public class VSwitchModelBehavior : ParameterBehavior<VSwitchModelBaseParameters>
    {
        /// <inheritdoc/>
        public new VSwitchModelBaseParameters Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBehavior{P}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public VSwitchModelBehavior(IBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<VSwitchModelBaseParameters>();
        }
    }
}
