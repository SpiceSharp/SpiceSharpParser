using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Validation;

namespace SpiceSharpParser.Analysis
{
    /// <summary>
    /// Result of a quick smoke test on a circuit netlist.
    /// </summary>
    public class SmokeTestResult
    {
        /// <summary>
        /// Gets or sets whether parsing succeeded.
        /// </summary>
        public bool ParseSuccess { get; set; }

        /// <summary>
        /// Gets the list of parse errors.
        /// </summary>
        public List<string> ParseErrors { get; } = new List<string>();

        /// <summary>
        /// Gets the lint result from structural validation.
        /// </summary>
        public LintResult LintResult { get; set; }

        /// <summary>
        /// Gets or sets whether the OP (operating point) simulation converged.
        /// </summary>
        public bool OPConverges { get; set; }

        /// <summary>
        /// Gets or sets the convergence error message if OP failed.
        /// </summary>
        public string ConvergenceError { get; set; }

        /// <summary>
        /// Gets the DC node voltages from the OP simulation.
        /// </summary>
        public Dictionary<string, double> NodeVoltages { get; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the device operating regions (BJT, MOSFET) from the OP simulation.
        /// </summary>
        public Dictionary<string, DeviceRegion> DeviceRegions { get; } = new Dictionary<string, DeviceRegion>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the SpiceSharpModel (available if parsing succeeded).
        /// </summary>
        public SpiceSharpModel Model { get; set; }

        /// <summary>
        /// Overall pass: parsing OK, no lint errors, and OP converges (if applicable).
        /// </summary>
        public bool IsPass => ParseSuccess
                           && (LintResult == null || !LintResult.HasErrors)
                           && OPConverges;

        /// <summary>
        /// Returns a human-readable diagnostic summary.
        /// </summary>
        public string DiagnosticSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Smoke Test Results ===");

            // Parse
            sb.AppendLine($"Parse: {(ParseSuccess ? "OK" : "FAILED")}");
            foreach (var err in ParseErrors)
            {
                sb.AppendLine($"  - {err}");
            }

            // Lint
            if (LintResult != null && LintResult.Issues.Count > 0)
            {
                sb.AppendLine($"Lint: {LintResult.Issues.Count} issue(s)");
                foreach (var issue in LintResult.Issues)
                {
                    sb.AppendLine($"  - {issue}");
                }
            }
            else if (LintResult != null)
            {
                sb.AppendLine("Lint: OK");
            }

            // OP
            sb.AppendLine($"OP Convergence: {(OPConverges ? "OK" : "FAILED")}");
            if (!string.IsNullOrEmpty(ConvergenceError))
            {
                sb.AppendLine($"  Error: {ConvergenceError}");
            }

            // Node voltages
            if (NodeVoltages.Count > 0)
            {
                sb.AppendLine("Node Voltages:");
                foreach (var kvp in NodeVoltages.OrderBy(k => k.Key))
                {
                    sb.AppendLine($"  {kvp.Key} = {kvp.Value:G6}V");
                }
            }

            // Device regions
            if (DeviceRegions.Count > 0)
            {
                sb.AppendLine("Device Operating Regions:");
                foreach (var kvp in DeviceRegions.OrderBy(k => k.Key))
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Performs a quick structural verification of a SPICE netlist.
    /// Runs parse → lint → OP simulation → device region check in one call.
    /// </summary>
    public static class SmokeTester
    {
        /// <summary>
        /// Performs a quick check on a netlist string: parse, lint, and OP convergence.
        /// </summary>
        /// <param name="netlist">The SPICE netlist text.</param>
        /// <returns>A <see cref="SmokeTestResult"/> with structured diagnostics.</returns>
        public static SmokeTestResult QuickCheck(string netlist)
        {
            if (netlist == null)
            {
                throw new ArgumentNullException(nameof(netlist));
            }

            var result = new SmokeTestResult();

            // Step 1: Parse
            SpiceSharpModel model;
            try
            {
                model = ParseNetlist(netlist);
                result.ParseSuccess = !model.ValidationResult.HasError;

                foreach (var error in model.ValidationResult.Errors)
                {
                    result.ParseErrors.Add(error.Message);
                }

                if (!result.ParseSuccess)
                {
                    return result;
                }

                result.Model = model;
            }
            catch (Exception ex)
            {
                result.ParseSuccess = false;
                result.ParseErrors.Add(ex.Message);
                return result;
            }

            // Step 2: Lint
            result.LintResult = NetlistLinter.Lint(model);

            // Step 3: Run OP simulation for bias point
            RunOPSimulation(model, result);

            return result;
        }

        /// <summary>
        /// Performs a quick check on an already-parsed SpiceSharpModel.
        /// </summary>
        public static SmokeTestResult QuickCheck(SpiceSharpModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var result = new SmokeTestResult
            {
                ParseSuccess = !model.ValidationResult.HasError,
                Model = model,
            };

            foreach (var error in model.ValidationResult.Errors)
            {
                result.ParseErrors.Add(error.Message);
            }

            if (!result.ParseSuccess)
            {
                return result;
            }

            result.LintResult = NetlistLinter.Lint(model);

            RunOPSimulation(model, result);

            return result;
        }

        private static SpiceSharpModel ParseNetlist(string netlist)
        {
            // Trim whitespace from indented multiline strings
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

        private static void RunOPSimulation(SpiceSharpModel model, SmokeTestResult result)
        {
            // Create a fresh OP simulation
            var op = new OpWithEvents("smoke_op");

            // Collect node names from the circuit
            var inspector = new CircuitInspector(model);
            var nodes = inspector.GetNodes();

            // Set up voltage exports
            var exports = new Dictionary<string, RealVoltageExport>(StringComparer.OrdinalIgnoreCase);
            foreach (string node in nodes)
            {
                if (node != "0")
                {
                    try
                    {
                        exports[node] = new RealVoltageExport(op, node);
                    }
                    catch
                    {
                        // Skip nodes that can't be exported
                    }
                }
            }

            // Hook up data export
            op.EventExportData += (sender, e) =>
            {
                foreach (var kvp in exports)
                {
                    try
                    {
                        result.NodeVoltages[kvp.Key] = kvp.Value.Value;
                    }
                    catch
                    {
                        result.NodeVoltages[kvp.Key] = double.NaN;
                    }
                }
            };

            try
            {
                var codes = op.Run(model.Circuit, -1);
                codes = op.InvokeEvents(codes);
                codes.ToArray(); // Force enumeration

                result.OPConverges = true;
            }
            catch (Exception ex)
            {
                result.OPConverges = false;
                result.ConvergenceError = ex.Message;
            }

            // Determine device operating regions
            if (result.OPConverges && result.NodeVoltages.Count > 0)
            {
                foreach (string compName in inspector.GetComponentNames())
                {
                    var info = inspector.GetComponentInfo(compName);
                    if (info == null)
                    {
                        continue;
                    }

                    if (info.Type == "BJT")
                    {
                        result.DeviceRegions[compName] = inspector.GetBJTRegion(compName, result.NodeVoltages);
                    }
                    else if (info.Type.StartsWith("MOSFET"))
                    {
                        result.DeviceRegions[compName] = inspector.GetMOSFETRegion(compName, result.NodeVoltages);
                    }
                }
            }
        }
    }
}
