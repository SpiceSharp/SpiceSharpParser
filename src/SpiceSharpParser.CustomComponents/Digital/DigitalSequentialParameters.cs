using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpiceSharpParser.CustomComponents.Digital
{
    /// <summary>
    /// Optional per-instance overrides shared by the built-in sequential models.
    /// </summary>
    public sealed class DigitalSequentialParameters
    {
        /// <summary>Gets or sets the switching threshold ratio. The default is 0.5.</summary>
        public double? LogicThresholdRatio { get; set; }

        /// <summary>Gets or sets the transport propagation delay in seconds.</summary>
        public double? PropagationDelay { get; set; }

        /// <summary>Gets or sets each input resistance to VSS in ohms.</summary>
        public double? InputResistance { get; set; }

        /// <summary>Gets or sets the enabled output resistance in ohms.</summary>
        public double? OutputResistance { get; set; }

        /// <summary>
        /// Gets or sets the disabled output resistance in ohms for components
        /// with output enable. The default is 1 TOhm.
        /// </summary>
        public double? DisabledOutputResistance { get; set; }

        /// <summary>Gets or sets the output capacitance to VSS in farads.</summary>
        public double? OutputCapacitance { get; set; }

        /// <summary>Gets or sets the state forcing resistance in ohms.</summary>
        public double? StateResistance { get; set; }

        /// <summary>Gets or sets the ideal state storage capacitance in farads.</summary>
        public double? StateCapacitance { get; set; }

        /// <summary>
        /// Gets or sets the initial state value. One-bit models accept 0 or 1;
        /// four-bit models accept values from 0 through 15.
        /// </summary>
        public int? InitialValue { get; set; }

        /// <summary>
        /// Gets or sets the deterministic priority for simultaneous asynchronous
        /// preset and clear. The default is <see cref="DigitalAsynchronousPriority.Clear"/>.
        /// </summary>
        public DigitalAsynchronousPriority? AsynchronousPriority { get; set; }

        internal IReadOnlyDictionary<string, string> ToOverrides(
            int maximumInitialValue,
            bool supportsAsynchronousPriority,
            bool supportsDisabledOutput)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            DigitalParameterOverrides.AddRatio(result, "VTH", LogicThresholdRatio);
            DigitalParameterOverrides.AddNonNegative(result, "TPD", PropagationDelay);
            DigitalParameterOverrides.AddPositive(result, "RIN", InputResistance);
            DigitalParameterOverrides.AddPositive(result, "ROUT", OutputResistance);
            DigitalParameterOverrides.AddPositive(result, "COUT", OutputCapacitance);
            DigitalParameterOverrides.AddPositive(result, "RSTATE", StateResistance);
            DigitalParameterOverrides.AddPositive(result, "CMEM", StateCapacitance);

            if (DisabledOutputResistance.HasValue && !supportsDisabledOutput)
            {
                throw new ArgumentException(
                    "DisabledOutputResistance is only valid for a component with output enable.",
                    nameof(DisabledOutputResistance));
            }

            if (supportsDisabledOutput)
            {
                DigitalParameterOverrides.AddPositive(
                    result,
                    "ROFF",
                    DisabledOutputResistance);
                double onResistance = OutputResistance ?? 50.0;
                double offResistance = DisabledOutputResistance ?? 1.0e12;
                if (offResistance <= onResistance)
                {
                    throw new ArgumentException(
                        "The disabled output resistance must be greater than the enabled output resistance.",
                        nameof(DisabledOutputResistance));
                }
            }

            if (InitialValue.HasValue)
            {
                if (InitialValue.Value < 0 || InitialValue.Value > maximumInitialValue)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(InitialValue),
                        InitialValue.Value,
                        $"The initial value must be between 0 and {maximumInitialValue}.");
                }

                result["IC"] = InitialValue.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (AsynchronousPriority.HasValue)
            {
                if (!supportsAsynchronousPriority)
                {
                    throw new ArgumentException(
                        "AsynchronousPriority is only valid for a component with preset and clear inputs.",
                        nameof(AsynchronousPriority));
                }

                if (!Enum.IsDefined(typeof(DigitalAsynchronousPriority), AsynchronousPriority.Value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(AsynchronousPriority),
                        AsynchronousPriority.Value,
                        "Unknown asynchronous priority.");
                }

                result["PRE_PRIORITY"] =
                    AsynchronousPriority.Value == DigitalAsynchronousPriority.Preset ? "1" : "0";
            }

            return result;
        }
    }
}
