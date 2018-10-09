using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    /// <summary>
    /// An interface for all statements processors.
    /// </summary>
    public interface IProcessor
    {
        Statements Process(Statements statements);
    }
}
