using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.CurrentExports;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
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
        /// <returns>
        /// A new export
        /// </returns>
        public override Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator, bool ignoreCaseForNodes)
        {
            if (parameters.Count != 1 || (!(parameters[0] is VectorParameter) && !(parameters[0] is SingleParameter)))
            {
                throw new Exception("Current exports should have a single parameter or vector parameter");
            }

            // Get the nodes
            Identifier componentIdentifier = null;
            if (parameters[0] is VectorParameter vector)
            {
                switch (vector.Elements.Count)
                {
                    case 0:
                        throw new Exception("Node expected");
                    case 1:
                        componentIdentifier = new StringIdentifier(vector.Elements[0].Image);
                        break;
                    default:
                        throw new Exception("Too many nodes specified");
                }
            }
            else
            {
                componentIdentifier = new StringIdentifier(parameters.GetString(0));
            }

            Export export = null;
            switch (type.ToLower())
            {
                case "i": export = new CurrentExport(simulation, componentIdentifier); break;
                case "ir": export = new CurrentRealExport(simulation, componentIdentifier); break;
                case "ii": export = new CurrentImaginaryExport(simulation, componentIdentifier); break;
                case "im": export = new CurrentMagnitudeExport(simulation, componentIdentifier); break;
                case "idb": export = new CurrentDecibelExport(simulation, componentIdentifier); break;
                case "ip": export = new CurrentPhaseExport(simulation, componentIdentifier); break;
            }

            return export;
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
