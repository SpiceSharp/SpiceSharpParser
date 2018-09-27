using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationEvaluators
    {
        ISimulationEvaluators CreateChildContainer(string subcircuitFullName);

        IEnumerable<string> GetExpressionNames();

        IEvaluator GetSimulationEvaluator(Simulation simulation);

        double EvaluateDouble(string expression);

        void AddCustomFunction(string name, List<string> arguments, string body);

        void SetNamedExpression(string expressionName, string expression);

        void SetParameter(string parameterName, double value);

        void SetParameter(string parameterName, string expression);

        void SetParameters(Dictionary<string, string> subcircuitParameters);

        void SetSeed(int seed);
    }
}