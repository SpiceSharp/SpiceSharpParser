using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationPreparations
    {
        void Prepare(Simulation simulation);

        void SetNodeSetVoltage(string nodeId, string expression);

        void SetICVoltage(string nodeId, string expression);

        void SetParameter(IEntity @object, string paramName, string expression, bool beforeTemperature, bool onload);

        void SetParameter(IEntity @object, string paramName, double value, bool beforeTemperature, bool onload);

        void SetParameter(IEntity @object, Simulation simulation, string paramName, double value, bool beforeTemperature, bool onload);

        void ExecuteTemperatureBehaviorBeforeLoad(IEntity @object);

        void ExecuteActionBeforeSetup(Action<Simulation> action);

        void ExecuteActionAfterSetup(Action<Simulation> action);
    }
}