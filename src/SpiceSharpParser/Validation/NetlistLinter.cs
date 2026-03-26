using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Validation
{
    /// <summary>
    /// Severity levels for lint issues.
    /// </summary>
    public enum LintSeverity
    {
        Error,
        Warning,
        Info,
    }

    /// <summary>
    /// Categories of lint issues.
    /// </summary>
    public enum LintCategory
    {
        FloatingNode,
        MissingDCPath,
        MissingModel,
        DuplicateComponent,
        MissingACMagnitude,
        MissingTranMaxStep,
        EmptyCircuit,
        NoSimulation,
        NoExports,
    }

    /// <summary>
    /// Represents a single lint issue found during structural validation.
    /// </summary>
    public class LintIssue
    {
        public LintIssue(LintSeverity severity, LintCategory category, string message, string nodeOrComponent = null, string suggestedFix = null)
        {
            Severity = severity;
            Category = category;
            Message = message;
            NodeOrComponent = nodeOrComponent;
            SuggestedFix = suggestedFix;
        }

        public LintSeverity Severity { get; }

        public LintCategory Category { get; }

        public string Message { get; }

        public string NodeOrComponent { get; }

        public string SuggestedFix { get; }

        public override string ToString()
        {
            string result = $"[{Severity}] {Message}";
            if (NodeOrComponent != null)
            {
                result += $" (at: {NodeOrComponent})";
            }

            if (SuggestedFix != null)
            {
                result += $" — Fix: {SuggestedFix}";
            }

            return result;
        }
    }

    /// <summary>
    /// Result of linting a netlist.
    /// </summary>
    public class LintResult
    {
        public List<LintIssue> Issues { get; } = new List<LintIssue>();

        public bool HasErrors => Issues.Any(i => i.Severity == LintSeverity.Error);

        public bool HasWarnings => Issues.Any(i => i.Severity == LintSeverity.Warning);

        public IEnumerable<LintIssue> Errors => Issues.Where(i => i.Severity == LintSeverity.Error);

        public IEnumerable<LintIssue> Warnings => Issues.Where(i => i.Severity == LintSeverity.Warning);

        public override string ToString()
        {
            if (Issues.Count == 0)
            {
                return "No issues found.";
            }

            return string.Join(Environment.NewLine, Issues.Select(i => i.ToString()));
        }
    }

    /// <summary>
    /// Pre-simulation structural validation for SpiceSharp circuit models.
    /// Detects common errors that would cause simulation failures.
    /// </summary>
    public static class NetlistLinter
    {
        /// <summary>
        /// Performs structural validation on a SpiceSharpModel and returns all found issues.
        /// </summary>
        /// <param name="model">The parsed and read SpiceSharp model.</param>
        /// <returns>A <see cref="LintResult"/> containing all detected issues.</returns>
        public static LintResult Lint(SpiceSharpModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var result = new LintResult();

            CheckEmptyCircuit(model, result);
            CheckDuplicateComponents(model, result);
            CheckMissingDCPathToGround(model, result);
            CheckMissingModels(model, result);
            CheckTranMaxStep(model, result);
            CheckNoSimulation(model, result);
            CheckNoExports(model, result);

            return result;
        }

        private static void CheckEmptyCircuit(SpiceSharpModel model, LintResult result)
        {
            if (model.Circuit == null || !model.Circuit.Any())
            {
                result.Issues.Add(new LintIssue(
                    LintSeverity.Error,
                    LintCategory.EmptyCircuit,
                    "Circuit contains no components.",
                    suggestedFix: "Add at least one component to the netlist."));
            }
        }

        private static void CheckDuplicateComponents(SpiceSharpModel model, LintResult result)
        {
            if (model.Circuit == null)
            {
                return;
            }

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entity in model.Circuit)
            {
                if (!names.Add(entity.Name))
                {
                    result.Issues.Add(new LintIssue(
                        LintSeverity.Error,
                        LintCategory.DuplicateComponent,
                        $"Duplicate component name: '{entity.Name}'.",
                        entity.Name,
                        $"Rename one of the duplicate '{entity.Name}' components."));
                }
            }
        }

        private static void CheckMissingDCPathToGround(SpiceSharpModel model, LintResult result)
        {
            if (model.Circuit == null)
            {
                return;
            }

            // Build adjacency graph of nodes connected through DC-path components.
            // DC-path components: R, L, V, I, switches, controlled sources, transmission lines, BJT, MOSFET, Diode, JFET.
            // Non-DC-path at DC: C (open circuit at DC).
            var dcConnected = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var allNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in model.Circuit)
            {
                if (entity is SpiceSharp.Components.Component component)
                {
                    var nodes = new List<string>();
                    for (int i = 0; i < component.Nodes.Count; i++)
                    {
                        nodes.Add(component.Nodes[i]);
                        allNodes.Add(component.Nodes[i]);
                    }

                    // Capacitors don't provide DC path
                    bool isDCPath = !(entity is Capacitor || entity is BehavioralCapacitor);

                    if (isDCPath && nodes.Count >= 2)
                    {
                        // For 2-terminal devices, connect nodes directly
                        // For multi-terminal devices (BJT, MOSFET), connect all terminals as potentially DC-connected
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            for (int j = i + 1; j < nodes.Count; j++)
                            {
                                AddConnection(dcConnected, nodes[i], nodes[j]);
                            }
                        }
                    }
                }
            }

            // Ensure ground node "0" is in the set
            allNodes.Add("0");

            // BFS from ground to find all reachable nodes
            var reachable = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();
            queue.Enqueue("0");
            reachable.Add("0");

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                if (dcConnected.TryGetValue(current, out var neighbors))
                {
                    foreach (string neighbor in neighbors)
                    {
                        if (reachable.Add(neighbor))
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            // Report unreachable nodes
            foreach (string node in allNodes)
            {
                if (!reachable.Contains(node) && node != "0")
                {
                    result.Issues.Add(new LintIssue(
                        LintSeverity.Error,
                        LintCategory.MissingDCPath,
                        $"Node '{node}' has no DC path to ground.",
                        node,
                        $"Add a high-value resistor: R_gnd_{node} {node} 0 1G"));
                }
            }
        }

        private static void AddConnection(Dictionary<string, HashSet<string>> graph, string a, string b)
        {
            if (!graph.TryGetValue(a, out var neighborsA))
            {
                neighborsA = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                graph[a] = neighborsA;
            }

            neighborsA.Add(b);

            if (!graph.TryGetValue(b, out var neighborsB))
            {
                neighborsB = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                graph[b] = neighborsB;
            }

            neighborsB.Add(a);
        }

        private static void CheckMissingModels(SpiceSharpModel model, LintResult result)
        {
            if (model.Circuit == null)
            {
                return;
            }

            // Collect all entity names (models are also entities in the circuit)
            var entityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entity in model.Circuit)
            {
                entityNames.Add(entity.Name);
            }

            // Check semiconductor devices reference existing models
            foreach (var entity in model.Circuit)
            {
                string modelRef = GetModelReference(entity);

                if (modelRef != null && !entityNames.Contains(modelRef))
                {
                    result.Issues.Add(new LintIssue(
                        LintSeverity.Error,
                        LintCategory.MissingModel,
                        $"Component '{entity.Name}' references model '{modelRef}' which is not defined.",
                        entity.Name,
                        $"Add a .MODEL {modelRef} <type>(...) statement."));
                }
            }
        }

        private static string GetModelReference(IEntity entity)
        {
            if (entity is BipolarJunctionTransistor bjt)
            {
                return bjt.Model;
            }

            if (entity is Diode diode)
            {
                return diode.Model;
            }

            if (entity is Mosfet1 mos1)
            {
                return mos1.Model;
            }

            if (entity is Mosfet2 mos2)
            {
                return mos2.Model;
            }

            if (entity is Mosfet3 mos3)
            {
                return mos3.Model;
            }

            if (entity is JFET jfet)
            {
                return jfet.Model;
            }

            return null;
        }

        private static void CheckTranMaxStep(SpiceSharpModel model, LintResult result)
        {
            // Check for transient simulations
            bool hasTran = model.Simulations.Any(s => s is TransientWithEvents);
            if (!hasTran)
            {
                return;
            }

            // Check if circuit has large capacitors (> 100µF)
            bool hasLargeCaps = false;
            foreach (var entity in model.Circuit)
            {
                if (entity is Capacitor cap)
                {
                    double capacitance = System.Math.Abs(cap.Parameters.Capacitance.Value);
                    if (capacitance > 100e-6)
                    {
                        hasLargeCaps = true;
                        break;
                    }
                }
            }

            if (!hasLargeCaps)
            {
                return;
            }

            // Warn about potential tmax issue
            result.Issues.Add(new LintIssue(
                LintSeverity.Warning,
                LintCategory.MissingTranMaxStep,
                "Transient simulation with large capacitors (>100µF) detected. Ensure tmax (4th .TRAN argument) is set to avoid inaccurate results.",
                suggestedFix: "Add tmax to .TRAN: .TRAN <step> <stop> <start> <tmax>. Rule: tmax <= 1/10 of AC period."));
        }

        private static void CheckNoSimulation(SpiceSharpModel model, LintResult result)
        {
            if (model.Simulations == null || model.Simulations.Count == 0)
            {
                result.Issues.Add(new LintIssue(
                    LintSeverity.Warning,
                    LintCategory.NoSimulation,
                    "No simulation commands found in netlist.",
                    suggestedFix: "Add an analysis command: .OP, .DC, .AC, .TRAN, or .NOISE"));
            }
        }

        private static void CheckNoExports(SpiceSharpModel model, LintResult result)
        {
            if (model.Simulations.Count > 0 && model.Exports.Count == 0)
            {
                result.Issues.Add(new LintIssue(
                    LintSeverity.Info,
                    LintCategory.NoExports,
                    "Simulation defined but no .SAVE or .PRINT exports. Only .MEAS results will be available.",
                    suggestedFix: "Add .SAVE V(<node>) or .PRINT directives to collect waveform data."));
            }
        }
    }
}
