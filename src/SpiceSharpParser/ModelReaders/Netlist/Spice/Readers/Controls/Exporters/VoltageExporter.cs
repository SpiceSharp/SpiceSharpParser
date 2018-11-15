using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Generates voltage <see cref="Export"/>.
    /// </summary>
    public class VoltageExporter : Exporter
    {
        /// <summary>
        /// Gets supported voltage exports.
        /// </summary>
        /// <returns>
        /// A list of supported voltage exports.
        /// </returns>
        public override ICollection<string> CreatedTypes => new List<string>() { "v", "vr", "vi", "vm", "vdb", "vp", "vph" };

        /// <summary>
        /// Creates a new voltage export
        /// </summary>
        /// <param name="type">A type of export</param>
        /// <param name="parameters">A parameters of export</param>
        /// <param name="simulation">A simulation for export</param>
        /// <returns>
        /// A new export
        /// </returns>
        public override Export CreateExport(string name, string type, ParameterCollection parameters,
            Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator componentNameGenerator,
            IObjectNameGenerator modelNameGenerator, IResultService result, SpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (parameters.Count != 1 || (!(parameters[0] is VectorParameter) && !(parameters[0] is SingleParameter)))
            {
                throw new WrongParameterException("Voltage exports should have vector or single parameter");
            }

            // Get the nodes
            string node, reference = null;
            string nodePath = null, referencePath = null;

            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new WrongParametersCountException("No nodes for voltage export. Node expected");
                    case 2:
                        referencePath = vector.Elements[1].Image;
                        reference = nodeNameGenerator.Parse(referencePath);
                        goto case 1;
                    case 1:
                        nodePath = vector.Elements[0].Image;
                        node = nodeNameGenerator.Parse(nodePath);
                        break;
                    default:
                        throw new WrongParametersCountException("Too many nodes specified for voltage export");
                }
            }
            else
            {
                nodePath = parameters.GetString(0);
                node = nodeNameGenerator.Parse(nodePath);
            }

            Export ve = null;
            switch (type.ToLower())
            {
                case "v":
                    ve = new VoltageExport(name, simulation, node, reference);
                    break;
                case "vr":
                    ve = new VoltageRealExport(name, simulation, node, reference);
                    break;
                case "vi":
                    ve = new VoltageImaginaryExport(name, simulation, node, reference);
                    break;
                case "vm":
                    ve = new VoltageMagnitudeExport(name, simulation, node, reference);
                    break;
                case "vdb":
                    ve = new VoltageDecibelExport(name, simulation, node, reference);
                    break;
                case "vph":
                case "vp":
                    ve = new VoltagePhaseExport(name, simulation, node, reference);
                    break;
            }

            return ve;
        }
    }
}
