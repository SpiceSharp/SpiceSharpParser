using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    public interface ISubcircuitDefinitionReader
    {
        /// <summary>
        /// Reades a subcircuit statement
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A processing context</param>
        void Read(SubCircuit statement, IReadingContext context);
    }
}
