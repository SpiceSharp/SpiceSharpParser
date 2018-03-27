using System.Collections.Generic;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Common;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters
{
    /// <summary>
    /// Base clas for all exporters
    /// </summary>
    public abstract class Exporter : IGenerator
    {
        public string TypeName => string.Join(".", GetSupportedTypes());

        /// <summary>
        /// Creates a new export
        /// </summary>
        /// <param name="type">A type of export</param>
        /// <param name="parameters">A parameters of export</param>
        /// <param name="simulation">A simulation for export</param>
        /// <param name="context">A context</param>
        /// <returns>
        /// A new export
        /// </returns>
        public abstract Export CreateExport(string type, ParameterCollection parameters, Simulation simulation, IProcessingContext context);

        /// <summary>
        /// Gets supported exports
        /// </summary>
        /// <returns>
        /// A list of supported exports
        /// </returns>
        public abstract ICollection<string> GetSupportedTypes();
    }
}
