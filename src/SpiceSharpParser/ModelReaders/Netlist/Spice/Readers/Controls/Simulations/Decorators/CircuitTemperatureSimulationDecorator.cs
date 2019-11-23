using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators
{
    public class CircuitTemperatureSimulationDecorator : ISimulationDecorator
    {
        private readonly double _circuitTemperature;

        public CircuitTemperatureSimulationDecorator(double circuitTemperature)
        {
            _circuitTemperature = circuitTemperature;
        }

        public BaseSimulation Decorate(BaseSimulation simulation)
        {
            EventHandler<LoadStateEventArgs> setState = (object sender, LoadStateEventArgs e) =>
            {
                if (e.State is BaseSimulationState rs)
                {
                    rs.Temperature = _circuitTemperature;
                }

                // TODO: What to do with complex state?
            };

            simulation.BeforeTemperature += setState;

            return simulation;
        }
    }
}