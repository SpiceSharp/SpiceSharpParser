using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationPreparations
    {
        void Prepare(BaseSimulation simulation);

        void SetNodeSetVoltage(string nodeId, string expression);

        void SetICVoltage(string nodeId, string expression);

        void SetParameter(Entity @object, string paramName, string expression, bool beforeTemperature, bool onload);

        void SetParameter(Entity @object, string paramName, double value, bool beforeTemperature, bool onload);

        void SetParameter(Entity @object, BaseSimulation simulation, string paramName, double value, bool beforeTemperature, bool onload);

        void ExecuteTemperatureBehaviorBeforeLoad(Entity @object);
    }
}
