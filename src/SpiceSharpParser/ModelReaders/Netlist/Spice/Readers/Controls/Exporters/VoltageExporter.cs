using System.Collections.Generic;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
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
        public virtual ICollection<string> CreatedTypes => new List<string> { "v", "vr", "vi", "vm", "vdb", "vp", "vph" };

        public override Export CreateExport(
            string name,
            string type,
            ParameterCollection parameters,
            EvaluationContext context,
            ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            if (parameters.Count != 1 || (!(parameters[0] is VectorParameter) && !(parameters[0] is SingleParameter)))
            {
                throw new SpiceSharpParserException("Voltage exports should have vector or single parameter", parameters.LineInfo);
            }

            // Get the nodes
            string node, reference = null;
            string nodePath = null, referencePath = null;

            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new SpiceSharpParserException("No nodes for voltage export. Node expected", vector.LineInfo);
                    case 2:
                        referencePath = vector.Elements[1].Image;
                        reference = context.NameGenerator.ParseNodeName(referencePath);
                        nodePath = vector.Elements[0].Image;
                        node = context.NameGenerator.ParseNodeName(nodePath);
                        break;
                    case 1:
                        nodePath = vector.Elements[0].Image;
                        node = context.NameGenerator.ParseNodeName(nodePath);
                        break;

                    default:
                        throw new SpiceSharpParserException("Too many nodes specified for voltage export", vector.LineInfo);
                }
            }
            else
            {
                nodePath = parameters.Get(0).Image;
                node = context.NameGenerator.ParseNodeName(nodePath);
            }

            Export ve = null;
            switch (type.ToLower())
            {
                case "v":
                    ve = new VoltageExport(name, context.Simulation, node, reference);
                    break;

                case "vr":
                    ve = new VoltageRealExport(name, context.Simulation, node, reference);
                    break;

                case "vi":
                    ve = new VoltageImaginaryExport(name, context.Simulation, node, reference);
                    break;

                case "vm":
                    ve = new VoltageMagnitudeExport(name, context.Simulation, node, reference);
                    break;

                case "vdb":
                    ve = new VoltageDecibelExport(name, context.Simulation, node, reference);
                    break;

                case "vph":
                case "vp":
                    ve = new VoltagePhaseExport(name, context.Simulation, node, reference);
                    break;
            }

            return ve;
        }
    }
}