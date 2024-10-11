using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class SimulationUpdateAction
    {
        public SimulationUpdateAction(Action<ISimulationWithEvents, SimulationEvaluationContexts> action)
        {
            Run = action;
        }

        public Action<ISimulationWithEvents, SimulationEvaluationContexts> Run { get; private set; }
    }
}