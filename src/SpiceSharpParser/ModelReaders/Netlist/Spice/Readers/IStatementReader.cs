using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Base interface for all statement readers.
    /// </summary>
    public interface IStatementReader
    {
        void Read(Statement statement, IReadingContext context);
    }
}