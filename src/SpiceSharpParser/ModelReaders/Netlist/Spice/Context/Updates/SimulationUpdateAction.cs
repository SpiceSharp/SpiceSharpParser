using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationUpdateAction
    {
        public SimulationUpdateAction(Action<Simulation, SimulationEvaluationContexts> action)
        {
            Run = action;
        }

        public Action<Simulation, SimulationEvaluationContexts> Run { get; private set; }
    }
}