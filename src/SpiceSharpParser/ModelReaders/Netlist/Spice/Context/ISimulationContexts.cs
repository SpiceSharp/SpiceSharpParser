using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationContexts
    {
        IEvaluator GetSimulationEvaluator(Simulation simulation);

        void SetICVoltage(string nodeName, string expression);

        void SetParameter(string paramName, double value, BaseSimulation simulation);

        void SetEntityParameter(string paramName, Entity @object, string expression, BaseSimulation simulation = null);

        void SetModelParameter(string paramName, Entity model, string expression, BaseSimulation simulation);
    }
}
