using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationPreparations
    {
        void Prepare(ISimulationWithEvents simulation);

        void SetNodeSetVoltage(string nodeId, string expression);

        void SetICVoltage(string nodeId, string expression);

        void SetParameterBeforeTemperature(IEntity @object, string paramName, string expression);

        void SetParameterBeforeTemperature(IEntity @object, string paramName, double value);

        void SetParameterBeforeTemperature(IEntity @object, string paramName, double value, ISimulationWithEvents simulation);

        void ExecuteActionBeforeSetup(Action<ISimulationWithEvents> action);

        void ExecuteActionAfterSetup(Action<ISimulationWithEvents> action);
    }
}