using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry of <see cref="ModelGenerator"/>.
    /// </summary>
    public class ModelGeneratorRegistry : BaseRegistry<ModelGenerator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityGeneratorRegistry"/> class.
        /// </summary>
        public ModelGeneratorRegistry()
        {
        }
    }
}
