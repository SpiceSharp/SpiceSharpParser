using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationUpdates
    {
        public SimulationUpdates()
        {
            ParameterUpdatesBeforeSetup = new List<SimulationUpdate>();
            ParameterUpdatesBeforeTemperature = new List<SimulationUpdate>();
            ParameterUpdatesBeforeLoad = new List<SimulationUpdate>();
        }

        public Simulation Simulation { get; set; }

        public List<SimulationUpdate> ParameterUpdatesBeforeSetup { get; protected set; }

        public List<SimulationUpdate> ParameterUpdatesBeforeTemperature { get; protected set; }

        public List<SimulationUpdate> ParameterUpdatesBeforeLoad { get; protected set; }
    }
}
