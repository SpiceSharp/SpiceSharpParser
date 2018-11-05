using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationUpdates
    {
        public SimulationUpdates()
        {
            ParameterUpdatesBeforeSetup = new List<SimulationUpdate>();
            ParameterUpdatesBeforeTemperature = new List<SimulationUpdate>();
        }

        public Simulation Simulation { get; set; }

        public List<SimulationUpdate> ParameterUpdatesBeforeSetup { get; protected set; }

        public List<SimulationUpdate> ParameterUpdatesBeforeTemperature { get; protected set; }
    }
}
