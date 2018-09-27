using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IEvaluatorsContainer
    {
        IEvaluatorsContainer CreateChildContainer(string containerName);

        IEnumerable<string> GetExpressionNames();

        IEvaluator GetSimulationEvaluator(Simulation simulation);

        IDictionary<Simulation, IEvaluator> GetEvaluators();

        double EvaluateDouble(string expression);

        void AddCustomFunction(string name, List<string> arguments, string body);

        void SetNamedExpression(string expressionName, string expressionBody);

        void SetParameter(string parameterName, double value);

        void SetParameter(string parameterName, string expression);

        void SetParameters(Dictionary<string, string> subcircuitParameters);

        void SetSeed(int seed);
    }
}