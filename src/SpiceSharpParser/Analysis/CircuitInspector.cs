using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Analysis
{
    /// <summary>
    /// Operating region for semiconductor devices.
    /// </summary>
    public enum DeviceRegion
    {
        Unknown,
        Active,
        Saturation,
        Cutoff,
        Linear,
        Forward,
        Reverse,
    }

    /// <summary>
    /// Information about a circuit component.
    /// </summary>
    public class ComponentInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string[] Nodes { get; set; }

        public string ModelName { get; set; }

        public Dictionary<string, double> Parameters { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// Provides programmatic inspection of a SpiceSharp circuit model.
    /// Supports topology queries, parameter access, and post-simulation diagnostics.
    /// </summary>
    public class CircuitInspector
    {
        private readonly SpiceSharpModel _model;

        public CircuitInspector(SpiceSharpModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Gets the underlying model.
        /// </summary>
        public SpiceSharpModel Model => _model;

        /// <summary>
        /// Gets all unique node names in the circuit.
        /// </summary>
        public IReadOnlyList<string> GetNodes()
        {
            var nodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in _model.Circuit)
            {
                if (entity is SpiceSharp.Components.Component component)
                {
                    for (int i = 0; i < component.Nodes.Count; i++)
                    {
                        nodes.Add(component.Nodes[i]);
                    }
                }
            }

            return nodes.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Gets all component names in the circuit (excluding model definitions).
        /// </summary>
        public IReadOnlyList<string> GetComponentNames()
        {
            var names = new List<string>();

            foreach (var entity in _model.Circuit)
            {
                if (entity is SpiceSharp.Components.Component)
                {
                    names.Add(entity.Name);
                }
            }

            return names;
        }

        /// <summary>
        /// Gets all component names connected to a specific node.
        /// </summary>
        public IReadOnlyList<string> GetComponentsConnectedTo(string node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var result = new List<string>();

            foreach (var entity in _model.Circuit)
            {
                if (entity is SpiceSharp.Components.Component component)
                {
                    for (int i = 0; i < component.Nodes.Count; i++)
                    {
                        if (string.Equals(component.Nodes[i], node, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(component.Name);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets detailed information about a specific component.
        /// </summary>
        public ComponentInfo GetComponentInfo(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var entity = _model.Circuit[name];
            if (entity == null)
            {
                return null;
            }

            var info = new ComponentInfo { Name = entity.Name };

            if (entity is SpiceSharp.Components.Component component)
            {
                var nodes = new string[component.Nodes.Count];
                for (int i = 0; i < component.Nodes.Count; i++)
                {
                    nodes[i] = component.Nodes[i];
                }

                info.Nodes = nodes;
            }

            // Determine type and extract parameters
            if (entity is Resistor resistor)
            {
                info.Type = "Resistor";
                info.Parameters["Resistance"] = resistor.Parameters.Resistance;
            }
            else if (entity is Capacitor capacitor)
            {
                info.Type = "Capacitor";
                info.Parameters["Capacitance"] = capacitor.Parameters.Capacitance;
            }
            else if (entity is Inductor inductor)
            {
                info.Type = "Inductor";
                info.Parameters["Inductance"] = inductor.Parameters.Inductance;
            }
            else if (entity is BipolarJunctionTransistor bjt)
            {
                info.Type = "BJT";
                info.ModelName = bjt.Model;
            }
            else if (entity is Diode diode)
            {
                info.Type = "Diode";
                info.ModelName = diode.Model;
            }
            else if (entity is Mosfet1 mos1)
            {
                info.Type = "MOSFET1";
                info.ModelName = mos1.Model;
            }
            else if (entity is Mosfet2 mos2)
            {
                info.Type = "MOSFET2";
                info.ModelName = mos2.Model;
            }
            else if (entity is Mosfet3 mos3)
            {
                info.Type = "MOSFET3";
                info.ModelName = mos3.Model;
            }
            else if (entity is JFET jfet)
            {
                info.Type = "JFET";
                info.ModelName = jfet.Model;
            }
            else if (entity is VoltageSource)
            {
                info.Type = "VoltageSource";
            }
            else if (entity is CurrentSource)
            {
                info.Type = "CurrentSource";
            }
            else
            {
                info.Type = entity.GetType().Name;
            }

            return info;
        }

        /// <summary>
        /// Gets the value of a passive component (R, L, C).
        /// </summary>
        /// <param name="name">Component name.</param>
        /// <returns>The component value, or NaN if not a passive component.</returns>
        public double GetComponentValue(string name)
        {
            var entity = _model.Circuit[name];

            if (entity is Resistor resistor)
            {
                return resistor.Parameters.Resistance;
            }

            if (entity is Capacitor capacitor)
            {
                return capacitor.Parameters.Capacitance;
            }

            if (entity is Inductor inductor)
            {
                return inductor.Parameters.Inductance;
            }

            return double.NaN;
        }

        /// <summary>
        /// Sets the value of a passive component (R, L, C).
        /// </summary>
        /// <param name="name">Component name.</param>
        /// <param name="value">New value.</param>
        /// <returns>True if the value was set successfully.</returns>
        public bool SetComponentValue(string name, double value)
        {
            var entity = _model.Circuit[name];

            if (entity is Resistor resistor)
            {
                resistor.Parameters.Resistance = value;
                return true;
            }

            if (entity is Capacitor capacitor)
            {
                capacitor.Parameters.Capacitance = value;
                return true;
            }

            if (entity is Inductor inductor)
            {
                inductor.Parameters.Inductance = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines the operating region of a BJT based on provided node voltages.
        /// </summary>
        /// <param name="name">BJT component name.</param>
        /// <param name="nodeVoltages">Dictionary of node name → DC voltage (e.g., from SmokeTester).</param>
        /// <returns>The device operating region.</returns>
        public DeviceRegion GetBJTRegion(string name, Dictionary<string, double> nodeVoltages)
        {
            if (nodeVoltages == null)
            {
                throw new ArgumentNullException(nameof(nodeVoltages));
            }

            var entity = _model.Circuit[name];
            if (!(entity is BipolarJunctionTransistor bjt))
            {
                return DeviceRegion.Unknown;
            }

            if (bjt.Nodes.Count < 3)
            {
                return DeviceRegion.Unknown;
            }

            string collector = bjt.Nodes[0];
            string baseNode = bjt.Nodes[1];
            string emitter = bjt.Nodes[2];

            double vc = GetNodeVoltage(collector, nodeVoltages);
            double vb = GetNodeVoltage(baseNode, nodeVoltages);
            double ve = GetNodeVoltage(emitter, nodeVoltages);

            if (double.IsNaN(vc) || double.IsNaN(vb) || double.IsNaN(ve))
            {
                return DeviceRegion.Unknown;
            }

            double vbe = vb - ve;
            double vbc = vb - vc;

            // Simplified region detection (actual threshold depends on model)
            const double vThreshold = 0.5;

            if (vbe > vThreshold && vbc < vThreshold)
            {
                return DeviceRegion.Active;
            }

            if (vbe > vThreshold && vbc > vThreshold)
            {
                return DeviceRegion.Saturation;
            }

            return DeviceRegion.Cutoff;
        }

        /// <summary>
        /// Determines the operating region of a MOSFET based on provided node voltages.
        /// </summary>
        /// <param name="name">MOSFET component name.</param>
        /// <param name="nodeVoltages">Dictionary of node name → DC voltage.</param>
        /// <param name="vth">Threshold voltage (default 0.7V for NMOS).</param>
        /// <returns>The device operating region.</returns>
        public DeviceRegion GetMOSFETRegion(string name, Dictionary<string, double> nodeVoltages, double vth = 0.7)
        {
            if (nodeVoltages == null)
            {
                throw new ArgumentNullException(nameof(nodeVoltages));
            }

            var entity = _model.Circuit[name];
            if (!(entity is SpiceSharp.Components.Component mosfet) || mosfet.Nodes.Count < 4)
            {
                return DeviceRegion.Unknown;
            }

            // MOSFET nodes: drain, gate, source, bulk
            string drain = mosfet.Nodes[0];
            string gate = mosfet.Nodes[1];
            string source = mosfet.Nodes[2];

            double vd = GetNodeVoltage(drain, nodeVoltages);
            double vg = GetNodeVoltage(gate, nodeVoltages);
            double vs = GetNodeVoltage(source, nodeVoltages);

            if (double.IsNaN(vd) || double.IsNaN(vg) || double.IsNaN(vs))
            {
                return DeviceRegion.Unknown;
            }

            double vgs = vg - vs;
            double vds = vd - vs;

            if (vgs < vth)
            {
                return DeviceRegion.Cutoff;
            }

            if (vds < vgs - vth)
            {
                return DeviceRegion.Linear;
            }

            return DeviceRegion.Saturation;
        }

        /// <summary>
        /// Gets a summary of the circuit: component counts by type.
        /// </summary>
        public Dictionary<string, int> GetComponentCounts()
        {
            var counts = new Dictionary<string, int>();

            foreach (var entity in _model.Circuit)
            {
                string type = GetEntityType(entity);

                if (!counts.ContainsKey(type))
                {
                    counts[type] = 0;
                }

                counts[type]++;
            }

            return counts;
        }

        private static double GetNodeVoltage(string node, Dictionary<string, double> voltages)
        {
            if (string.Equals(node, "0", StringComparison.OrdinalIgnoreCase))
            {
                return 0.0;
            }

            if (voltages.TryGetValue(node, out double v))
            {
                return v;
            }

            return double.NaN;
        }

        private static string GetEntityType(IEntity entity)
        {
            if (entity is Resistor || entity is BehavioralResistor)
            {
                return "Resistor";
            }

            if (entity is Capacitor || entity is BehavioralCapacitor)
            {
                return "Capacitor";
            }

            if (entity is Inductor)
            {
                return "Inductor";
            }

            if (entity is BipolarJunctionTransistor)
            {
                return "BJT";
            }

            if (entity is Diode)
            {
                return "Diode";
            }

            if (entity is Mosfet1 || entity is Mosfet2 || entity is Mosfet3)
            {
                return "MOSFET";
            }

            if (entity is JFET)
            {
                return "JFET";
            }

            if (entity is VoltageSource)
            {
                return "VoltageSource";
            }

            if (entity is CurrentSource)
            {
                return "CurrentSource";
            }

            if (entity is SpiceSharp.Components.Component)
            {
                return "Other";
            }

            return "Model";
        }
    }
}
