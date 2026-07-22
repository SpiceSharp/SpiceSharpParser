using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpiceSharpParser
{
    /// <summary>
    /// Describes a reusable subcircuit definition loaded from SPICE source.
    /// </summary>
    public sealed class SpiceSubcircuitInfo
    {
        internal SpiceSubcircuitInfo(
            string name,
            IEnumerable<string> pins,
            IDictionary<string, string> defaultParameters)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pins = new List<string>(pins ?? throw new ArgumentNullException(nameof(pins))).AsReadOnly();
            DefaultParameters = new ReadOnlyDictionary<string, string>(
                defaultParameters ?? throw new ArgumentNullException(nameof(defaultParameters)));
        }

        /// <summary>
        /// Gets the subcircuit name used by an SPICE X instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ordered external pin names from the .SUBCKT declaration.
        /// </summary>
        public IReadOnlyList<string> Pins { get; }

        /// <summary>
        /// Gets the default parameter expressions declared by the subcircuit.
        /// </summary>
        public IReadOnlyDictionary<string, string> DefaultParameters { get; }
    }
}
