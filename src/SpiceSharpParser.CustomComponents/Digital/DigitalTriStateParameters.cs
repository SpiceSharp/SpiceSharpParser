using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Optional per-instance overrides for the built-in tri-state models.
    /// </summary>
    public sealed class DigitalTriStateParameters
    {
        /// <summary>Gets or sets the switching threshold ratio. The default is 0.5.</summary>
        public double? LogicThresholdRatio { get; set; }

        /// <summary>Gets or sets the data and enable transport delay in seconds.</summary>
        public double? PropagationDelay { get; set; }

        /// <summary>Gets or sets each input resistance to VSS in ohms.</summary>
        public double? InputResistance { get; set; }

        /// <summary>Gets or sets the enabled output resistance in ohms.</summary>
        public double? OnResistance { get; set; }

        /// <summary>Gets or sets the disabled leakage resistance to VSS in ohms.</summary>
        public double? OffResistance { get; set; }

        /// <summary>Gets or sets the output capacitance to VSS in farads.</summary>
        public double? OutputCapacitance { get; set; }

        internal IReadOnlyDictionary<string, string> ToOverrides()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            DigitalParameterOverrides.AddRatio(result, "VTH", LogicThresholdRatio);
            DigitalParameterOverrides.AddNonNegative(result, "TPD", PropagationDelay);
            DigitalParameterOverrides.AddPositive(result, "RIN", InputResistance);
            DigitalParameterOverrides.AddPositive(result, "RON", OnResistance);
            DigitalParameterOverrides.AddPositive(result, "ROFF", OffResistance);
            DigitalParameterOverrides.AddPositive(result, "COUT", OutputCapacitance);

            double onResistance = OnResistance ?? 50.0;
            double offResistance = OffResistance ?? 1.0e12;
            if (offResistance <= onResistance)
            {
                throw new ArgumentException(
                    "The disabled resistance must be greater than the enabled resistance.",
                    nameof(OffResistance));
            }

            return result;
        }
    }
}
