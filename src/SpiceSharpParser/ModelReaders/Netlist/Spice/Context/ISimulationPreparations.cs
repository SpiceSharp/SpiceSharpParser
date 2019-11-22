using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationPreparations
    {
        void Prepare(BaseSimulation simulation);

        void SetNodeSetVoltage(string nodeId, string expression, ICircuitContext circuitContext);

        void SetICVoltage(string nodeId, string expression, ICircuitContext circuitContext);

        void SetParameter(Entity @object, string paramName, string expression, bool beforeTemperature, bool onload, ICircuitContext circuitContext);

        void SetParameter(Entity @object, string paramName, double value, bool beforeTemperature, bool onload);

        void SetParameter(Entity @object, BaseSimulation simulation, string paramName, double value, bool beforeTemperature, bool onload);

        void ExecuteTemperatureBehaviorBeforeLoad(Entity @object);

        void ExecuteActionBeforeSetup(Action<BaseSimulation> action);
    }
}