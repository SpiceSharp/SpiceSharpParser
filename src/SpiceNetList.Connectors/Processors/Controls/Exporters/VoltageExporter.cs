using System;
using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters.VoltageExports;
using SpiceSharp;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Simulations;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters
{
    /// <summary>
    /// Generates voltage <see cref="Export"/>
    /// </summary>
    public class VoltageExporter
    {
        /// <summary>
        /// Creates a new voltage export
        /// </summary>
        /// <param name="type">A type of export</param>
        /// <param name="parameters">A parameters of export</param>
        /// <param name="simulation">A simulation for export</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new export
        /// </returns>
        public Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, ProcessingContext context)
        {
            if (parameters.Count != 1 || (!(parameters[0] is VectorParameter) && !(parameters[0] is SingleParameter)))
            {
                throw new Exception("Voltage exports should have vector or single parameter");
            }

            // Get the nodes
            Identifier node, reference = null;
            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new Exception("Node expected");
                    case 2:
                        reference = new Identifier(context.GenerateNodeName(vector.Elements[1].Image));
                        goto case 1;
                    case 1:
                        node = new Identifier(context.GenerateNodeName(vector.Elements[0].Image));
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                node = new Identifier(context.GenerateNodeName(parameters.GetString(0)));
            }

            Export ve = null;
            switch (type.ToLower())
            {
                case "v": ve = new VoltageExport(simulation, node, reference); break;
                case "vr": ve = new VoltageRealExport(simulation, node, reference); break;
                case "vi": ve = new VoltageImaginaryExport(simulation, node, reference); break;
                case "vm": ve = new VoltageMagnitudeExport(simulation, node, reference); break;
                case "vdb": ve = new VoltageDecibelExport(simulation, node, reference); break;
                case "vph":
                case "vp": ve = new VoltagePhaseExport(simulation, node, reference); break;
            }

            return ve;
        }

        /// <summary>
        /// Gets supported voltage exports
        /// </summary>
        /// <returns>
        /// A list of supported voltage exports
        /// </returns>
        public List<string> GetGeneratedTypes()
        {
            return new List<string>() { "v", "vr", "vi", "vm", "vdb", "vp", "vph" };
        }
    }
}
