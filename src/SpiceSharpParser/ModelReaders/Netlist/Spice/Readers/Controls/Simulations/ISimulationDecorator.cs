using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    public interface ISimulationDecorator
    {
        ISimulationWithEvents Decorate(ISimulationWithEvents simulation);
    }
}