using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
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

        public ISimulationWithEvents Decorate(ISimulationWithEvents simulation)
        {
            OnBeforeTemperature setState = (object sender, TemperatureStateEventArgs e) =>
            {
                if (e != null)
                {
                    e.State.Temperature = _circuitTemperature;
                }
            };

            if (simulation is ISimulationWithEvents biasingSimulation)
            {
                biasingSimulation.EventBeforeTemperature += setState;
            }

            return simulation;
        }
    }
}