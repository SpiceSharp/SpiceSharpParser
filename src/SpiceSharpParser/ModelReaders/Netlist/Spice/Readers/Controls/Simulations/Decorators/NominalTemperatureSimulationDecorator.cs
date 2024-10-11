using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class NominalTemperatureSimulationDecorator : ISimulationDecorator
    {
        private readonly double _nominalTemperatureInKelvins;

        public NominalTemperatureSimulationDecorator(double nominalTemperatureInKelvins)
        {
            _nominalTemperatureInKelvins = nominalTemperatureInKelvins;
        }

        public ISimulationWithEvents Decorate(ISimulationWithEvents simulation)
        {
            OnBeforeTemperature setState = (_, e) =>
            {
                e.State.NominalTemperature = _nominalTemperatureInKelvins;
            };

            if (simulation is BiasingSimulation biasingSimulation)
            {
                simulation.EventBeforeTemperature += setState;
            }

            return simulation;
        }
    }
}