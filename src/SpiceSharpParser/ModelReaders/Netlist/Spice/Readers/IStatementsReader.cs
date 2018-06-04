using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    public interface IStatementsReader
    {
        void Read(Statements statements, IReadingContext context);
    }
}
