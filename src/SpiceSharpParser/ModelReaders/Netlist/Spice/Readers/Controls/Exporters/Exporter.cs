using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Base clas for all exporters.
    /// </summary>
    public abstract class Exporter
    {
        /// <summary>
        /// Creates a new export
        /// </summary>
        /// <param name="type">A type of export</param>
        /// <param name="parameters">A parameters of export</param>
        /// <param name="simulation">A simulation for export</param>
        /// <returns>
        /// A new export.
        /// </returns>
        public abstract Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator, bool ignoreCaseForNodes);

        /// <summary>
        /// Gets supported exports.
        /// </summary>
        /// <returns>
        /// A list of supported exports.
        /// </returns>
        public abstract ICollection<string> GetSupportedTypes();

        protected string GetNodeName(string image, bool ignoreCaseForNodes)
        {
            return ignoreCaseForNodes ? image.ToLower() : image;
        }
    }
}
