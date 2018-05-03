using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters
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
        /// <param name="context">A context</param>
        /// <returns>
        /// A new export
        /// </returns>
        public override Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, IProcessingContext context)
        {
            if (parameters.Count != 2)
            {
                throw new WrongParameterException("Voltage exports should have two parameters: name of component and property name");
            }

            return new PropertyExport(simulation, new StringIdentifier(parameters[0].Image), parameters[1].Image);
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
