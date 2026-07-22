using System;
using System.Collections.Generic;
using System.Globalization;

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

            AddRatio(result, "VTH", LogicThresholdRatio);
            AddNonNegative(result, "TPD", PropagationDelay);
            AddPositive(result, "RIN", InputResistance);
            AddPositive(result, "ROUT", OutputResistance);
            AddPositive(result, "COUT", OutputCapacitance);

            return result;
        }

        private static void AddRatio(
            IDictionary<string, string> destination,
            string name,
            double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            ValidateFinite(name, value.Value);
            if (value.Value <= 0.0 || value.Value >= 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value.Value,
                    "The logic threshold ratio must be greater than zero and less than one.");
            }

            destination[name] = Format(value.Value);
        }

        private static void AddPositive(
            IDictionary<string, string> destination,
            string name,
            double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            ValidateFinite(name, value.Value);
            if (value.Value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value.Value,
                    "The value must be greater than zero.");
            }

            destination[name] = Format(value.Value);
        }

        private static void AddNonNegative(
            IDictionary<string, string> destination,
            string name,
            double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            ValidateFinite(name, value.Value);
            if (value.Value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value.Value,
                    "The value must not be negative.");
            }

            destination[name] = Format(value.Value);
        }

        private static void ValidateFinite(string name, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "The value must be finite.");
            }
        }

        private static string Format(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
