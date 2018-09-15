using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    public interface IStatementsReader
    {
        void Read(Statements statements, IReadingContext context, IStatementsOrderer orderer);
    }
}
