using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Measurements;

namespace SpiceSharpParser.Analysis
{
    /// <summary>
    /// Sensitivity data for a single component.
    /// </summary>
    public class ComponentSensitivity
    {
        public string ComponentName { get; set; }

        public double NominalComponentValue { get; set; }

        /// <summary>
        /// Normalized sensitivity: (∂spec/spec) / (∂component/component).
        /// A value of 1.0 means a 1% change in the component causes a 1% change in the spec.
        /// </summary>
        public double Sensitivity { get; set; }

        /// <summary>
        /// Spec value at -perturbation.
        /// </summary>
        public double SpecAtMinus { get; set; }

        /// <summary>
        /// Spec value at +perturbation.
        /// </summary>
        public double SpecAtPlus { get; set; }
    }

    /// <summary>
    /// Result of a sensitivity analysis for one measurement.
    /// </summary>
    public class SensitivityResult
    {
        public string MeasurementName { get; set; }

        public double NominalValue { get; set; }

        public Dictionary<string, ComponentSensitivity> Sensitivities { get; } = new Dictionary<string, ComponentSensitivity>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns sensitivities sorted by absolute impact (highest first).
        /// </summary>
        public List<(string Component, double Sensitivity)> RankedByImpact()
        {
            return Sensitivities.Values
                .OrderByDescending(s => Math.Abs(s.Sensitivity))
                .Select(s => (s.ComponentName, s.Sensitivity))
                .ToList();
        }
    }

    /// <summary>
    /// Computes sensitivity of .MEAS results to passive component values.
    /// Uses central finite differences: perturb each component ±X% and measure the effect.
    /// </summary>
    public class SensitivityAnalyzer
    {
        private readonly string _netlist;

        /// <summary>
        /// Initializes a new SensitivityAnalyzer with a netlist string.
        /// </summary>
        /// <param name="netlist">SPICE netlist text.</param>
        public SensitivityAnalyzer(string netlist)
        {
            _netlist = netlist ?? throw new ArgumentNullException(nameof(netlist));
        }

        /// <summary>
        /// Computes the sensitivity of a specific measurement to all passive components.
        /// </summary>
        /// <param name="measurementName">The .MEAS result name to analyze.</param>
        /// <param name="perturbationPct">Perturbation percentage (default 1.0 = ±1%).</param>
        /// <returns>A <see cref="SensitivityResult"/> with per-component sensitivities.</returns>
        public SensitivityResult ComputeSensitivity(string measurementName, double perturbationPct = 1.0)
        {
            if (measurementName == null)
            {
                throw new ArgumentNullException(nameof(measurementName));
            }

            var result = new SensitivityResult { MeasurementName = measurementName };

            // Run nominal simulation
            double nominalValue = RunAndGetMeasurement(_netlist, measurementName);
            result.NominalValue = nominalValue;

            if (double.IsNaN(nominalValue))
            {
                return result; // Measurement doesn't exist or failed
            }

            // Parse to find passive components
            var nominalModel = ParseModel(_netlist);
            var inspector = new CircuitInspector(nominalModel);
            var componentNames = inspector.GetComponentNames();

            double fraction = perturbationPct / 100.0;

            foreach (string compName in componentNames)
            {
                double value = inspector.GetComponentValue(compName);
                if (double.IsNaN(value) || value == 0)
                {
                    continue; // Skip non-passive or zero-value components
                }

                // Create perturbed models
                double valueMinus = value * (1.0 - fraction);
                double valuePlus = value * (1.0 + fraction);

                double specMinus = RunWithModifiedComponent(compName, valueMinus, measurementName);
                double specPlus = RunWithModifiedComponent(compName, valuePlus, measurementName);

                if (double.IsNaN(specMinus) || double.IsNaN(specPlus))
                {
                    continue;
                }

                // Normalized sensitivity: (∂spec/spec) / (∂comp/comp)
                double dSpec = specPlus - specMinus;
                double dComp = valuePlus - valueMinus;
                double sensitivity = 0;

                if (Math.Abs(nominalValue) > 1e-15 && Math.Abs(value) > 1e-15)
                {
                    sensitivity = (dSpec / nominalValue) / (dComp / value);
                }
                else if (Math.Abs(dComp) > 1e-15)
                {
                    // Use absolute sensitivity if nominal is near zero
                    sensitivity = dSpec / dComp;
                }

                result.Sensitivities[compName] = new ComponentSensitivity
                {
                    ComponentName = compName,
                    NominalComponentValue = value,
                    Sensitivity = sensitivity,
                    SpecAtMinus = specMinus,
                    SpecAtPlus = specPlus,
                };
            }

            return result;
        }

        /// <summary>
        /// Computes sensitivity for a specific component only.
        /// </summary>
        public double ComputePartialDerivative(string measurementName, string componentName, double perturbationPct = 1.0)
        {
            var fullResult = ComputeSensitivity(measurementName, perturbationPct);

            if (fullResult.Sensitivities.TryGetValue(componentName, out var sensitivity))
            {
                return sensitivity.Sensitivity;
            }

            return double.NaN;
        }

        private double RunWithModifiedComponent(string componentName, double newValue, string measurementName)
        {
            try
            {
                var model = ParseModel(_netlist);
                var inspector = new CircuitInspector(model);

                if (!inspector.SetComponentValue(componentName, newValue))
                {
                    return double.NaN;
                }

                return RunSimulationAndGetMeasurement(model, measurementName);
            }
            catch
            {
                return double.NaN;
            }
        }

        private double RunAndGetMeasurement(string netlist, string measurementName)
        {
            try
            {
                var model = ParseModel(netlist);
                return RunSimulationAndGetMeasurement(model, measurementName);
            }
            catch
            {
                return double.NaN;
            }
        }

        private static double RunSimulationAndGetMeasurement(SpiceSharpModel model, string measurementName)
        {
            // Run all simulations
            foreach (var simulation in model.Simulations)
            {
                try
                {
                    var exports = model.Exports.Where(ex => ex.Simulation == simulation).ToList();
                    simulation.EventExportData += (sender, e) =>
                    {
                        foreach (var export in exports)
                        {
                            try
                            {
                                export.Extract();
                            }
                            catch
                            {
                                // Ignore export errors
                            }
                        }
                    };

                    var codes = simulation.Run(model.Circuit, -1);
                    codes = simulation.InvokeEvents(codes);
                    codes.ToArray();
                }
                catch
                {
                    // Simulation failed — continue to next
                }
            }

            // Extract measurement result
            if (model.Measurements.TryGetValue(measurementName, out var results))
            {
                var successResult = results.FirstOrDefault(r => r.Success);
                if (successResult != null)
                {
                    return successResult.Value;
                }
            }

            return double.NaN;
        }

        private static SpiceSharpModel ParseModel(string netlist)
        {
            var trimmed = string.Join(
                Environment.NewLine,
                netlist.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(trimmed);

            var reader = new SpiceSharpReader();
            return reader.Read(parseResult.FinalModel);
        }
    }
}
