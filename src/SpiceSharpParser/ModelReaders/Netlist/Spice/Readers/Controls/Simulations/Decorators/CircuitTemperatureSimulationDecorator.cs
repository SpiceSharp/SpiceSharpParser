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

        public Simulation Decorate(Simulation simulation)
        {
            EventHandler<TemperatureStateEventArgs> setState = (object sender, TemperatureStateEventArgs e) =>
            {
              e.State.Temperature = _circuitTemperature;
            };

            if (simulation is BiasingSimulation biasingSimulation)
            {
                biasingSimulation.BeforeTemperature += setState;
            }

            return simulation;
        }
    }
}