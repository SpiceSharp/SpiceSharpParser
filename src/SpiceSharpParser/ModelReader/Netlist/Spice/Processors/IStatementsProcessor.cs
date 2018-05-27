using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    public interface IStatementsProcessor
    {
        void Process(Statements statements, IProcessingContext context);

        T GetRegistry<T>(); //TODO: refactor this, move to somewhere else
    }
}
