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

        void SetParameterBeforeTemperature(IEntity @object, string paramName, string expression);

        void SetParameterBeforeTemperature(IEntity @object, string paramName, double value);

        void SetParameterBeforeTemperature(IEntity @object, string paramName, double value, Simulation simulation);

        void ExecuteActionBeforeSetup(Action<Simulation> action);

        void ExecuteActionAfterSetup(Action<Simulation> action);
    }
}