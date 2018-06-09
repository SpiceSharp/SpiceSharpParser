using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Common;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.EntityGenerators
{
    /// <summary>
    /// Entity generator.
    /// </summary>
    public abstract class EntityGenerator : ISpiceObjectReader
    {
        /// <summary>
        /// Gets the spice name.
        /// </summary>
        public string SpiceName => string.Join(".", GetGeneratedSpiceTypes());

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
        /// Generated Spice type.
        /// </returns>
        public abstract IEnumerable<string> GetGeneratedSpiceTypes();
    }
}
