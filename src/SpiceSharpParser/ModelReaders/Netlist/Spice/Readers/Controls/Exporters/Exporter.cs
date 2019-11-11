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
        /// Creates a new export.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="type">A type of export.</param>
        /// <param name="parameters">A parameters of export.</param>
        /// <param name="simulation">A simulation for export.</param>
        /// <param name="nodeNameGenerator">Node name generator.</param>
        /// <param name="componentNameGenerator">Component name generator.</param>
        /// <param name="modelNameGenerator">Model name generator.</param>
        /// <param name="caseSettings">Case settings.</param>
        /// <returns>
        /// A new export.
        /// </returns>
        public abstract Export CreateExport(string name, string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator componentNameGenerator, IObjectNameGenerator modelNameGenerator, IResultService resultService, SpiceNetlistCaseSensitivitySettings caseSettings);
    }
}
