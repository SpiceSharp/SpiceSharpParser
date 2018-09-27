using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationsParameters
    {
        void Prepare(BaseSimulation simulation);

        void SetICVoltage(string nodeName, string voltageExpression);

        void SetNodeSetVoltage(string nodeName, string voltageExpression);

        void SetParameter(Entity entity, string paramName, string expression, int order);

        void SetParameter(Entity entity, string paramName, string expression, BaseSimulation simulation, int order);

        void SetParameter(Entity entity, string paramName, double value, int order);

        void SetParameter(Entity entity, string paramName, double value, BaseSimulation simulation, int order);
    }
}
