using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    public interface ISubcircuitDefinitionReader
    {
        /// <summary>
        /// Reads a subcircuit statement
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A reading context</param>
        void Read(SubCircuit statement, IReadingContext context);
    }
}