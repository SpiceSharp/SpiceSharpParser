using System;
using System.Collections.Generic;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Controls.Exporters.CurrentExports;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;
using SpiceSharp;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters
{
    /// <summary>
    /// Generates a current <see cref="Export"/>
    /// </summary>
    public class CurrentExporter : Exporter
    {
        /// <summary>
        /// Creates a new current export
        /// </summary>
        /// <param name="type">A type of export</param>
        /// <param name="parameters">A parameters of export</param>
        /// <param name="simulation">A simulation for export</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new export
        /// </returns>
        public override Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, IProcessingContext context)
        {
            if (parameters.Count != 1 || !(parameters[0] is SingleParameter))
            {
                throw new Exception("Current exports should single parameter");
            }

            // Get the nodes
            Identifier node, reference = null;
            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new Exception("Node expected");
                    case 1:
                        node = new StringIdentifier(context.NodeNameGenerator.Generate(vector.Elements[0].Image));
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                node = new StringIdentifier(context.NodeNameGenerator.Generate(parameters.GetString(0)));
            }

            Export ce = null;
            switch (type.ToLower())
            {
                case "i": ce = new CurrentExport(simulation, node); break;
                case "ir": ce = new CurrentRealExport(simulation, node); break;
                case "ii": ce = new CurrentImaginaryExport(simulation, node); break;
                case "im": ce = new CurrentMagnitudeExport(simulation, node); break;
                case "idb": ce = new CurrentDecibelExport(simulation, node); break;
                case "ip": ce = new CurrentPhaseExport(simulation, node); break;
            }

            return ce;
        }

        /// <summary>
        /// Gets supported current exports
        /// </summary>
        /// <returns>
        /// A list of supported current exports
        /// </returns>
        public override ICollection<string> GetSupportedTypes()
        {
            return new List<string>() { "i", "ir", "ii", "im", "idb", "ip" };
        }
    }
}
