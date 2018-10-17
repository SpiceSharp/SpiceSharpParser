using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationsParameters
    {
        void Prepare(BaseSimulation simulation);

        void SetICVoltage(string nodeId, string voltageExpression);

        void SetNodeSetVoltage(string nodeId, string voltageExpression);

        void SetParameter(Entity entity, string paramName, string expression, int order, bool onload = true, IEqualityComparer<string> comparer = null);

        void SetParameter(Entity entity, string paramName, double value, int order, bool onload = true, IEqualityComparer<string> comparer = null);

        void SetParameter(Entity entity, string paramName, string expression, BaseSimulation simulation, int order, bool onload = true, IEqualityComparer<string> comparer = null);

        void SetParameter(Entity entity, string paramName, double value, BaseSimulation simulation, int order, IEqualityComparer<string> comparer = null);
    }
}
