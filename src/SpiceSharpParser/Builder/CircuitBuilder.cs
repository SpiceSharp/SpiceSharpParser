using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Builder
{
    /// <summary>
    /// Fluent builder for constructing SPICE netlists programmatically.
    /// Eliminates string manipulation errors and enables programmatic circuit modification.
    /// </summary>
    public class CircuitBuilder
    {
        private string _title = "Circuit";
        private readonly List<string> _components = new List<string>();
        private readonly List<string> _models = new List<string>();
        private readonly List<string> _analyses = new List<string>();
        private readonly List<string> _outputs = new List<string>();
        private readonly List<string> _controls = new List<string>();
        private readonly Dictionary<string, int> _componentIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sets the netlist title.
        /// </summary>
        public CircuitBuilder Title(string title)
        {
            _title = title ?? "Circuit";
            return this;
        }

        // ────────────────────────────────────────────
        // Passive components
        // ────────────────────────────────────────────

        public CircuitBuilder Resistor(string name, string node1, string node2, double value)
        {
            AddComponent(name, $"{name} {node1} {node2} {FormatValue(value)}");
            return this;
        }

        public CircuitBuilder Capacitor(string name, string node1, string node2, double value)
        {
            AddComponent(name, $"{name} {node1} {node2} {FormatValue(value)}");
            return this;
        }

        public CircuitBuilder Inductor(string name, string node1, string node2, double value)
        {
            AddComponent(name, $"{name} {node1} {node2} {FormatValue(value)}");
            return this;
        }

        public CircuitBuilder MutualInductance(string name, string inductor1, string inductor2, double coupling)
        {
            AddComponent(name, $"{name} {inductor1} {inductor2} {FormatValue(coupling)}");
            return this;
        }

        // ────────────────────────────────────────────
        // Independent sources
        // ────────────────────────────────────────────

        public CircuitBuilder VoltageSource(string name, string posNode, string negNode, double dc = 0, double ac = 0)
        {
            var parts = new List<string> { name, posNode, negNode };

            if (dc != 0)
            {
                parts.Add($"DC {FormatValue(dc)}");
            }

            if (ac != 0)
            {
                parts.Add($"AC {FormatValue(ac)}");
            }

            if (dc == 0 && ac == 0)
            {
                parts.Add("0");
            }

            AddComponent(name, string.Join(" ", parts));
            return this;
        }

        public CircuitBuilder VoltageSourceSine(string name, string posNode, string negNode, double offset, double amplitude, double frequency, double delay = 0, double damping = 0)
        {
            string sine = $"SIN({FormatValue(offset)} {FormatValue(amplitude)} {FormatValue(frequency)}";
            if (delay != 0 || damping != 0)
            {
                sine += $" {FormatValue(delay)} {FormatValue(damping)}";
            }

            sine += ")";

            AddComponent(name, $"{name} {posNode} {negNode} {sine}");
            return this;
        }

        public CircuitBuilder VoltageSourcePulse(string name, string posNode, string negNode, double v1, double v2, double delay, double rise, double fall, double width, double period)
        {
            string pulse = $"PULSE({FormatValue(v1)} {FormatValue(v2)} {FormatValue(delay)} {FormatValue(rise)} {FormatValue(fall)} {FormatValue(width)} {FormatValue(period)})";
            AddComponent(name, $"{name} {posNode} {negNode} {pulse}");
            return this;
        }

        public CircuitBuilder VoltageSourcePWL(string name, string posNode, string negNode, params (double time, double value)[] points)
        {
            string pwl = "PWL(" + string.Join(" ", points.Select(p => $"{FormatValue(p.time)} {FormatValue(p.value)}")) + ")";
            AddComponent(name, $"{name} {posNode} {negNode} {pwl}");
            return this;
        }

        public CircuitBuilder CurrentSource(string name, string posNode, string negNode, double dc = 0, double ac = 0)
        {
            var parts = new List<string> { name, posNode, negNode };

            if (dc != 0)
            {
                parts.Add($"DC {FormatValue(dc)}");
            }

            if (ac != 0)
            {
                parts.Add($"AC {FormatValue(ac)}");
            }

            if (dc == 0 && ac == 0)
            {
                parts.Add("0");
            }

            AddComponent(name, string.Join(" ", parts));
            return this;
        }

        // ────────────────────────────────────────────
        // Semiconductor devices
        // ────────────────────────────────────────────

        public CircuitBuilder Diode(string name, string anode, string cathode, string modelName)
        {
            AddComponent(name, $"{name} {anode} {cathode} {modelName}");
            return this;
        }

        public CircuitBuilder BJT(string name, string collector, string baseNode, string emitter, string modelName)
        {
            AddComponent(name, $"{name} {collector} {baseNode} {emitter} {modelName}");
            return this;
        }

        public CircuitBuilder MOSFET(string name, string drain, string gate, string source, string bulk, string modelName, double? length = null, double? width = null)
        {
            var line = $"{name} {drain} {gate} {source} {bulk} {modelName}";

            if (length.HasValue)
            {
                line += $" L={FormatValue(length.Value)}";
            }

            if (width.HasValue)
            {
                line += $" W={FormatValue(width.Value)}";
            }

            AddComponent(name, line);
            return this;
        }

        public CircuitBuilder JFET(string name, string drain, string gate, string source, string modelName)
        {
            AddComponent(name, $"{name} {drain} {gate} {source} {modelName}");
            return this;
        }

        // ────────────────────────────────────────────
        // Controlled sources
        // ────────────────────────────────────────────

        /// <summary>
        /// Voltage-Controlled Voltage Source (E element).
        /// </summary>
        public CircuitBuilder VCVS(string name, string posOut, string negOut, string posCtrl, string negCtrl, double gain)
        {
            AddComponent(name, $"{name} {posOut} {negOut} {posCtrl} {negCtrl} {FormatValue(gain)}");
            return this;
        }

        /// <summary>
        /// Voltage-Controlled Current Source (G element).
        /// </summary>
        public CircuitBuilder VCCS(string name, string posOut, string negOut, string posCtrl, string negCtrl, double transconductance)
        {
            AddComponent(name, $"{name} {posOut} {negOut} {posCtrl} {negCtrl} {FormatValue(transconductance)}");
            return this;
        }

        /// <summary>
        /// Behavioral voltage source (B element).
        /// </summary>
        public CircuitBuilder BehavioralVoltageSource(string name, string posNode, string negNode, string expression)
        {
            AddComponent(name, $"{name} {posNode} {negNode} V={{{expression}}}");
            return this;
        }

        /// <summary>
        /// Behavioral current source (B element).
        /// </summary>
        public CircuitBuilder BehavioralCurrentSource(string name, string posNode, string negNode, string expression)
        {
            AddComponent(name, $"{name} {posNode} {negNode} I={{{expression}}}");
            return this;
        }

        // ────────────────────────────────────────────
        // Models
        // ────────────────────────────────────────────

        public CircuitBuilder Model(string name, string type, Dictionary<string, double> parameters)
        {
            var paramStr = string.Join(" ", parameters.Select(p => $"{p.Key}={FormatValue(p.Value)}"));
            _models.Add($".MODEL {name} {type}({paramStr})");
            return this;
        }

        public CircuitBuilder ModelRaw(string modelLine)
        {
            _models.Add(modelLine);
            return this;
        }

        // ────────────────────────────────────────────
        // Analysis commands
        // ────────────────────────────────────────────

        public CircuitBuilder OP()
        {
            _analyses.Add(".OP");
            return this;
        }

        public CircuitBuilder DC(string sourceName, double start, double stop, double step)
        {
            _analyses.Add($".DC {sourceName} {FormatValue(start)} {FormatValue(stop)} {FormatValue(step)}");
            return this;
        }

        public CircuitBuilder AC(string sweepType, int points, double fStart, double fStop)
        {
            _analyses.Add($".AC {sweepType} {points} {FormatValue(fStart)} {FormatValue(fStop)}");
            return this;
        }

        public CircuitBuilder Tran(double step, double stop, double start = 0, double? maxStep = null, bool uic = false)
        {
            var parts = new List<string> { ".TRAN", FormatValue(step), FormatValue(stop) };

            if (start != 0 || maxStep.HasValue)
            {
                parts.Add(FormatValue(start));
            }

            if (maxStep.HasValue)
            {
                parts.Add(FormatValue(maxStep.Value));
            }

            if (uic)
            {
                parts.Add("UIC");
            }

            _analyses.Add(string.Join(" ", parts));
            return this;
        }

        // ────────────────────────────────────────────
        // Output commands
        // ────────────────────────────────────────────

        public CircuitBuilder Save(params string[] exports)
        {
            _outputs.Add(".SAVE " + string.Join(" ", exports));
            return this;
        }

        public CircuitBuilder Meas(string analysisType, string name, string expression)
        {
            _outputs.Add($".MEAS {analysisType} {name} {expression}");
            return this;
        }

        public CircuitBuilder Print(string analysisType, params string[] exports)
        {
            _outputs.Add($".PRINT {analysisType} " + string.Join(" ", exports));
            return this;
        }

        // ────────────────────────────────────────────
        // Control statements
        // ────────────────────────────────────────────

        public CircuitBuilder Param(string name, string expression)
        {
            _controls.Add($".PARAM {name}={expression}");
            return this;
        }

        public CircuitBuilder IC(string node, double voltage)
        {
            _controls.Add($".IC V({node})={FormatValue(voltage)}");
            return this;
        }

        public CircuitBuilder Options(string options)
        {
            _controls.Add($".OPTIONS {options}");
            return this;
        }

        public CircuitBuilder Subcircuit(string definition)
        {
            _controls.Add(definition);
            return this;
        }

        public CircuitBuilder RawLine(string line)
        {
            _controls.Add(line);
            return this;
        }

        // ────────────────────────────────────────────
        // Modification
        // ────────────────────────────────────────────

        /// <summary>
        /// Sets the value of an existing passive component (R, L, C).
        /// Replaces the component line in the builder.
        /// </summary>
        public CircuitBuilder SetValue(string componentName, double newValue)
        {
            if (!_componentIndex.TryGetValue(componentName, out int index))
            {
                throw new ArgumentException($"Component '{componentName}' not found.", nameof(componentName));
            }

            string oldLine = _components[index];
            // Parse the existing line to find and replace the value
            string[] parts = oldLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4)
            {
                // Standard format: <name> <node1> <node2> <value>
                parts[3] = FormatValue(newValue);
                _components[index] = string.Join(" ", parts);
            }

            return this;
        }

        /// <summary>
        /// Removes a component from the builder.
        /// </summary>
        public CircuitBuilder RemoveComponent(string name)
        {
            if (_componentIndex.TryGetValue(name, out int index))
            {
                _components[index] = null; // Mark as removed
                _componentIndex.Remove(name);
            }

            return this;
        }

        // ────────────────────────────────────────────
        // Build
        // ────────────────────────────────────────────

        /// <summary>
        /// Generates the SPICE netlist string.
        /// </summary>
        public string ToNetlist()
        {
            var sb = new StringBuilder();

            sb.AppendLine(_title);

            // Components
            foreach (var comp in _components)
            {
                if (comp != null)
                {
                    sb.AppendLine(comp);
                }
            }

            // Models
            foreach (var model in _models)
            {
                sb.AppendLine(model);
            }

            // Controls
            foreach (var ctrl in _controls)
            {
                sb.AppendLine(ctrl);
            }

            // Analyses
            foreach (var analysis in _analyses)
            {
                sb.AppendLine(analysis);
            }

            // Outputs
            foreach (var output in _outputs)
            {
                sb.AppendLine(output);
            }

            sb.AppendLine(".END");

            return sb.ToString();
        }

        /// <summary>
        /// Parses and reads the netlist in one step.
        /// </summary>
        public SpiceSharpModel Build()
        {
            string netlist = ToNetlist();

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(netlist);

            var reader = new SpiceSharpReader();
            return reader.Read(parseResult.FinalModel);
        }

        // ────────────────────────────────────────────
        // Static factory
        // ────────────────────────────────────────────

        /// <summary>
        /// Creates a new CircuitBuilder with a title.
        /// </summary>
        public static CircuitBuilder Create(string title = "Circuit")
        {
            return new CircuitBuilder().Title(title);
        }

        // ────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────

        private void AddComponent(string name, string line)
        {
            if (_componentIndex.ContainsKey(name))
            {
                // Replace existing
                _components[_componentIndex[name]] = line;
            }
            else
            {
                _componentIndex[name] = _components.Count;
                _components.Add(line);
            }
        }

        private static string FormatValue(double value)
        {
            double abs = Math.Abs(value);

            if (abs == 0)
            {
                return "0";
            }

            if (abs >= 1e12)
            {
                return (value / 1e12).ToString("G6", CultureInfo.InvariantCulture) + "T";
            }

            if (abs >= 1e9)
            {
                return (value / 1e9).ToString("G6", CultureInfo.InvariantCulture) + "G";
            }

            if (abs >= 1e6)
            {
                return (value / 1e6).ToString("G6", CultureInfo.InvariantCulture) + "MEG";
            }

            if (abs >= 1e3)
            {
                return (value / 1e3).ToString("G6", CultureInfo.InvariantCulture) + "k";
            }

            if (abs >= 1)
            {
                return value.ToString("G6", CultureInfo.InvariantCulture);
            }

            if (abs >= 1e-3)
            {
                return (value / 1e-3).ToString("G6", CultureInfo.InvariantCulture) + "m";
            }

            if (abs >= 1e-6)
            {
                return (value / 1e-6).ToString("G6", CultureInfo.InvariantCulture) + "u";
            }

            if (abs >= 1e-9)
            {
                return (value / 1e-9).ToString("G6", CultureInfo.InvariantCulture) + "n";
            }

            if (abs >= 1e-12)
            {
                return (value / 1e-12).ToString("G6", CultureInfo.InvariantCulture) + "p";
            }

            return (value / 1e-15).ToString("G6", CultureInfo.InvariantCulture) + "f";
        }
    }
}
