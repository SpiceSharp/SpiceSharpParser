using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Optional per-instance overrides for the built-in digital gate models.
    /// </summary>
    public sealed class DigitalGateParameters
    {
        /// <summary>
        /// Gets or sets the switching threshold as a fraction of the VDD-to-VSS voltage.
        /// The built-in default is 0.5.
        /// </summary>
        public double? LogicThresholdRatio { get; set; }

        /// <summary>
        /// Gets or sets the transport propagation delay in seconds.
        /// The built-in default is 10 ns.
        /// </summary>
        public double? PropagationDelay { get; set; }

        /// <summary>
        /// Gets or sets each input resistance to VSS in ohms.
        /// The built-in default is 1 GOhm.
        /// </summary>
        public double? InputResistance { get; set; }

        /// <summary>
        /// Gets or sets the series output resistance in ohms.
        /// The built-in default is 50 ohms.
        /// </summary>
        public double? OutputResistance { get; set; }

        /// <summary>
        /// Gets or sets the intrinsic output capacitance to VSS in farads.
        /// The built-in default is 5 pF.
        /// </summary>
        public double? OutputCapacitance { get; set; }

        internal IReadOnlyDictionary<string, string> ToOverrides()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            DigitalParameterOverrides.AddRatio(result, "VTH", LogicThresholdRatio);
            DigitalParameterOverrides.AddNonNegative(result, "TPD", PropagationDelay);
            DigitalParameterOverrides.AddPositive(result, "RIN", InputResistance);
            DigitalParameterOverrides.AddPositive(result, "ROUT", OutputResistance);
            DigitalParameterOverrides.AddPositive(result, "COUT", OutputCapacitance);

            return result;
        }
    }
}
