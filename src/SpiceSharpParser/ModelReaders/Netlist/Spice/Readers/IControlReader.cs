using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all control readers.
    /// </summary>
    public interface IControlReader
    {
        /// <summary>
        /// Reads a control statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context.</param>
        void Read(Control statement, IReadingContext context);
    }
}