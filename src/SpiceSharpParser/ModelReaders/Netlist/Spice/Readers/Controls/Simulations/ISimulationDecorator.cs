using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations
{
    public interface ISimulationDecorator
    {
        Simulation Decorate(Simulation simulation);
    }
}