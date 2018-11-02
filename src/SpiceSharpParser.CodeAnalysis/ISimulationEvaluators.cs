using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationEvaluatorsContainer
    {
        IEvaluator GetSimulationEvaluator(Simulation simulation);

        IDictionary<Simulation, IEvaluator> GetEvaluators();
    }
}