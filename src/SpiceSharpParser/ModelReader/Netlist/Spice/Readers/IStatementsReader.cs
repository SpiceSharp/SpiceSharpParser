using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
{
    public interface IStatementsReader
    {
        void Read(Statements statements, IReadingContext context);
    }
}
