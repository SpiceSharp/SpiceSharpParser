using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Base class for all exporters.
    /// </summary>
    public abstract class Exporter
    {
        /// <summary>
        /// Gets created exports.
        /// </summary>
        /// <returns>
        /// A list of created exports.
        /// </returns>
        public abstract ICollection<string> CreatedTypes { get; }

        /// <summary>
        /// Creates a new export.
        /// </summary>
        /// <paramref name="name">Name of export.</paramref>
        /// <param name="type">A type of export.</param>
        /// <param name="parameters">A parameters of export.</param>
        /// <param name="simulation">A simulation for export.</param>
        /// <returns>
        /// A new export.
        /// </returns>
        public abstract Export CreateExport(string name, string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator componentNameGenerator, IObjectNameGenerator modelNameGenerator, IResultService result, SpiceNetlistCaseSensitivitySettings caseSettings);
    }
}
