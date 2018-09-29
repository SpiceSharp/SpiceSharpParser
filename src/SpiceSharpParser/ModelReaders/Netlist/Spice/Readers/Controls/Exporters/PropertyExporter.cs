using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Generates a property <see cref="Export"/>
    /// </summary>
    public class PropertyExporter : Exporter
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
            if (parameters.Count != 2)
            {
                throw new WrongParameterException("Property exports should have two parameters: name of component and property name");
            }

            return new PropertyExport(simulation, new StringIdentifier(parameters[0].Image), parameters[1].Image.ToLower());
        }

        /// <summary>
        /// Gets supported property exports
        /// </summary>
        /// <returns>
        /// A list of supported current exports
        /// </returns>
        public override ICollection<string> GetSupportedTypes()
        {
            return new List<string>() { "@" };
        }
    }
}
