using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationUpdate
    {
        public Simulation Simulation { get; set; }

        public Action<Simulation, SimulationEvaluationContexts> Update { get; set; }
    }
}