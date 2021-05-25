using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class NominalTemperatureSimulationDecorator : ISimulationDecorator
    {
        private readonly double _nominalTemperatureInKelvins;

        public NominalTemperatureSimulationDecorator(double nominalTemperatureInKelvins)
        {
            _nominalTemperatureInKelvins = nominalTemperatureInKelvins;
        }

        public Simulation Decorate(Simulation simulation)
        {
            EventHandler<TemperatureStateEventArgs> setState = (_, e) =>
            {
                e.State.NominalTemperature = _nominalTemperatureInKelvins;
            };

            if (simulation is BiasingSimulation biasingSimulation)
            {
                biasingSimulation.BeforeTemperature += setState;
            }

            return simulation;
        }
    }
}