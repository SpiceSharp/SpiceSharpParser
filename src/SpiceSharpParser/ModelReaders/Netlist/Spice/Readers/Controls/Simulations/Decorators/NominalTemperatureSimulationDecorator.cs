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

        public BaseSimulation Decorate(BaseSimulation simulation)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is BaseSimulationState rs)
                {
                    rs.NominalTemperature = _nominalTemperatureInKelvins;
                }

                // TODO: What to do with complex state?
            };

            simulation.BeforeTemperature += setState;

            return simulation;
        }
    }
}