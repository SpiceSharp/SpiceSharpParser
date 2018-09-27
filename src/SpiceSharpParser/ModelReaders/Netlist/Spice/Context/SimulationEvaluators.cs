using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationEvaluators : ISimulationEvaluators
    {
        protected Dictionary<Simulation, IEvaluator> Evaluators = new Dictionary<Simulation, IEvaluator>();

        protected IEvaluator SourceEvaluator { get; }

        public SimulationEvaluators(IEvaluator sourceEvaluator)
        {
            SourceEvaluator = sourceEvaluator;
        }

        public IEvaluator GetSimulationEvaluator(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!Evaluators.ContainsKey(simulation))
            {
                Evaluators[simulation] = 
                    SourceEvaluator.CreateClonedEvaluator(simulation.Name.ToString(), simulation, SourceEvaluator.Seed);
            }

            return Evaluators[simulation];
        }

        public void AddCustomFunction(string name, List<string> args, string body)
        {
            SourceEvaluator.AddCustomFunction(name, args, body);

            foreach (var evaluator in Evaluators.Values)
            {
                evaluator.AddCustomFunction(name, args, body);
            }
        }

        public void SetNamedExpression(string expressionName, string expression)
        {
            SourceEvaluator.SetNamedExpression(expressionName, expression);

            foreach (var evaluator in Evaluators.Values)
            {
                evaluator.SetNamedExpression(expressionName, expression);
            }
        }

        public void SetParameter(string parameterName, string expression)
        {
            SourceEvaluator.SetParameter(parameterName, expression);

            foreach (var evaluator in Evaluators.Values)
            {
                evaluator.SetParameter(parameterName, expression);
            }
        }

        public void SetParameter(string parameterName, double value)
        {
            SourceEvaluator.SetParameter(parameterName, value);

            foreach (var evaluator in Evaluators.Values)
            {
                evaluator.SetParameter(parameterName, value);
            }
        }

        public double EvaluateDouble(string expression)
        {
            return SourceEvaluator.EvaluateDouble(expression);
        }

        public ISimulationEvaluators CreateChildContainer(string containerName)
        {
            return new SimulationEvaluators(SourceEvaluator.CreateChildEvaluator(containerName, null));
        }

        public void SetParameters(Dictionary<string, string> parameterWithValues)
        {
            foreach (var parameterWithValue in parameterWithValues)
            {
                SetParameter(parameterWithValue.Key, parameterWithValue.Value);
            }
        }

        public IEnumerable<string> GetExpressionNames()
        {
            return SourceEvaluator.GetExpressionNames();
        }

        internal IDictionary<Simulation, IEvaluator> GetEvaluators()
        {
            return Evaluators;
        }

        public void SetSeed(int seed)
        {
            SourceEvaluator.Seed = seed;

            foreach (var evaluator in Evaluators.Values)
            {
                evaluator.Seed = ++seed;
            }
        }
    }
}
