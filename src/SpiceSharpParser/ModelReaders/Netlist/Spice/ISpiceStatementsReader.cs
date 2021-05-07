using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    /// <summary>
    /// An interface for all statements readers.
    /// </summary>
    public interface ISpiceStatementsReader
    {
        /// <summary>
        /// Reads a statement.
        /// </summary>
        /// <param name="statement">A statement.</param>
        /// <param name="circuitContext">A reading context.</param>
        void Read(Statement statement, IReadingContext circuitContext);
    }
}