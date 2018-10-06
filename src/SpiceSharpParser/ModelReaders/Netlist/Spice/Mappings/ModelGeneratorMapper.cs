using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Mapper for elements of <see cref="IModelGenerator"/> class.
    /// </summary>
    public class ModelGeneratorMapper : BaseMapper<IModelGenerator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelGeneratorMapper"/> class.
        /// </summary>
        public ModelGeneratorMapper()
        {
        }
    }
}
