using System.Collections.Generic;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Processors.Common;
using SpiceSharp;
using SpiceSharp.Circuits;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Entity generator
    /// </summary>
    public abstract class EntityGenerator : IGenerator
    {
        /// <summary>
        /// Gets the type of generator
        /// </summary>
        public string TypeName => string.Join(".", GetGeneratedSpiceTypes());

        /// <summary>
        /// Generates entity
        /// </summary>
        /// <param name="id">The identifier for identity</param>
        /// <param name="originalName">Original name of enity</param>
        /// <param name="type">The type of entity</param>
        /// <param name="parameters">Parameters for entity</param>
        /// <param name="context">Processing context</param>
        /// <returns>
        /// A new instance of entity
        /// </returns>
        public abstract Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context);

        /// <summary>
        /// Gets generated Spice types by generator
        /// </summary>
        /// <returns>
        /// Generated Spice Types
        /// </returns>
        public abstract IEnumerable<string> GetGeneratedSpiceTypes();
    }
}
