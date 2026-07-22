using System;
using System.Collections.Generic;

namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Optional per-instance overrides for the built-in Schmitt-trigger models.
    /// </summary>
    public sealed class DigitalSchmittParameters
    {
        /// <summary>Gets or sets the rising threshold ratio. The default is 0.65.</summary>
        public double? RisingThresholdRatio { get; set; }

        /// <summary>Gets or sets the falling threshold ratio. The default is 0.35.</summary>
        public double? FallingThresholdRatio { get; set; }

        /// <summary>Gets or sets the transport propagation delay in seconds.</summary>
        public double? PropagationDelay { get; set; }

        /// <summary>Gets or sets the input resistance to VSS in ohms.</summary>
        public double? InputResistance { get; set; }

        /// <summary>Gets or sets the series output resistance in ohms.</summary>
        public double? OutputResistance { get; set; }

        /// <summary>Gets or sets the output capacitance to VSS in farads.</summary>
        public double? OutputCapacitance { get; set; }

        /// <summary>Gets or sets the state forcing resistance in ohms.</summary>
        public double? StateResistance { get; set; }

        /// <summary>Gets or sets the state hold resistance in ohms.</summary>
        public double? HoldResistance { get; set; }

        /// <summary>Gets or sets the state storage capacitance in farads.</summary>
        public double? StateCapacitance { get; set; }

        internal IReadOnlyDictionary<string, string> ToOverrides()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            DigitalParameterOverrides.AddRatio(result, "VTH_RISE", RisingThresholdRatio);
            DigitalParameterOverrides.AddRatio(result, "VTH_FALL", FallingThresholdRatio);

            double rising = RisingThresholdRatio ?? 0.65;
            double falling = FallingThresholdRatio ?? 0.35;
            if (rising <= falling)
            {
                throw new ArgumentException(
                    "The rising threshold ratio must be greater than the falling threshold ratio.",
                    nameof(RisingThresholdRatio));
            }

            DigitalParameterOverrides.AddNonNegative(result, "TPD", PropagationDelay);
            DigitalParameterOverrides.AddPositive(result, "RIN", InputResistance);
            DigitalParameterOverrides.AddPositive(result, "ROUT", OutputResistance);
            DigitalParameterOverrides.AddPositive(result, "COUT", OutputCapacitance);
            DigitalParameterOverrides.AddPositive(result, "RSTATE", StateResistance);
            DigitalParameterOverrides.AddPositive(result, "RHOLD", HoldResistance);
            DigitalParameterOverrides.AddPositive(result, "CMEM", StateCapacitance);

            return result;
        }
    }
}
