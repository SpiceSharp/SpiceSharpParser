using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators
{
    /// <summary>
    /// Entity generator.
    /// </summary>
    public abstract class EntityGenerator
    {
        /// <summary>
        /// Generates entity.
        /// </summary>
        /// <param name="id">The identifier for identity.</param>
        /// <param name="originalName">Original name of enity.</param>
        /// <param name="type">The type of entity.</param>
        /// <param name="parameters">Parameters for entity.</param>
        /// <param name="context">Reading context.</param>
        /// <returns>
        /// A new instance of entity.
        /// </returns>
        public abstract Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IReadingContext context);

        /// <summary>
        /// Gets generated Spice types by generator.
        /// </summary>
        /// <returns>
        /// Generated SPICE type.
        /// </returns>
        public abstract IEnumerable<string> GetGeneratedTypes();
    }
}
