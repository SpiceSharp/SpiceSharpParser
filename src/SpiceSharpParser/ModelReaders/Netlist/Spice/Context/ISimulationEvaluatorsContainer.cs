using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface ISimulationEvaluatorsContainer
    {
        ISimulationEvaluatorsContainer CreateChildContainer(string containerName);

        IEnumerable<string> GetExpressionNames();

        IEvaluator GetSimulationEvaluator(Simulation simulation);

        IEvaluator GetSimulationEntityEvaluator(Simulation simulation, string entityName);

        IDictionary<Simulation, IEvaluator> GetEvaluators();

        double EvaluateDouble(string expression);

        double EvaluateDouble(string expression, Simulation simulation);

        void AddFunction(string name, List<string> arguments, string body);

        void SetNamedExpression(string expressionName, string expressionBody);

        void SetParameter(string parameterName, double value);

        void SetParameter(string parameterName, string expression);

        void SetParameters(Dictionary<string, string> subcircuitParameters);

        void UpdateSeed(int? seed);

        bool IsConstantExpression(string expression);
    }
}