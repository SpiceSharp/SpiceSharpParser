using System;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class NominalTemperatureSimulationDecorator
    {
        public static BaseSimulation Decorate(BaseSimulation simulation, double nominalTemperatureInKelvins)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is RealState rs)
                {
                    rs.NominalTemperature = nominalTemperatureInKelvins;
                }

                //TODO: What to do with complex state?
            };

            simulation.BeforeTemperature += setState;

            return simulation;
        }
    }
}
