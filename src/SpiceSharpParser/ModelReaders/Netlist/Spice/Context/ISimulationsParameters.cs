using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationsParameters
    {
        void Prepare(BaseSimulation simulation);

        void SetICVoltage(SimulationExpressionContexts contexts, string nodeId, string voltageExpression);

        void SetNodeSetVoltage(SimulationExpressionContexts contexts, string nodeId, string voltageExpression);

        void SetParameter(SimulationExpressionContexts contexts, Entity entity, string paramName, string expression, int order, bool onload = true, IEqualityComparer<string> comparer = null);

        void SetParameter(SimulationExpressionContexts contexts, Entity entity, string paramName, double value, int order, bool onload = true, IEqualityComparer<string> comparer = null);

        void SetParameter(SimulationExpressionContexts contexts, Entity entity, string paramName, string expression, BaseSimulation simulation, int order, bool onload = true, IEqualityComparer<string> comparer = null);

        void SetParameter(SimulationExpressionContexts contexts, Entity entity, string paramName, double value, BaseSimulation simulation, int order, IEqualityComparer<string> comparer = null);
    }
}