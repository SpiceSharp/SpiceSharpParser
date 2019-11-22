using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    public interface ISimulationDecorator
    {
        BaseSimulation Decorate(BaseSimulation simulation);
    }
}