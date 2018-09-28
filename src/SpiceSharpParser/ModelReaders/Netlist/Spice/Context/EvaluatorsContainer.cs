using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class EvaluatorsContainer : IEvaluatorsContainer
    {
        protected Dictionary<Simulation, IEvaluator> Evaluators = new Dictionary<Simulation, IEvaluator>();

        public EvaluatorsContainer(IEvaluator sourceEvaluator)
        {
            SourceEvaluator = sourceEvaluator ?? throw new ArgumentNullException(nameof(sourceEvaluator));
        }

        protected IEvaluator SourceEvaluator { get; }

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

        public void SetParameters(Dictionary<string, string> parameterWithValues)
        {
            foreach (var parameterWithValue in parameterWithValues)
            {
                SetParameter(parameterWithValue.Key, parameterWithValue.Value);
            }
        }

        public double EvaluateDouble(string expression)
        {
            return SourceEvaluator.EvaluateDouble(expression);
        }

        public double EvaluateDouble(string expression, Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            return GetSimulationEvaluator(simulation).EvaluateDouble(expression);
        }

        public IEvaluatorsContainer CreateChildContainer(string containerName)
        {
            return new EvaluatorsContainer(SourceEvaluator.CreateChildEvaluator(containerName, null));
        }

        public IEnumerable<string> GetExpressionNames()
        {
            return SourceEvaluator.GetExpressionNames();
        }

        public IDictionary<Simulation, IEvaluator> GetEvaluators()
        {
            return Evaluators;
        }

        public IEvaluator GetSimulationEvaluator(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!Evaluators.ContainsKey(simulation))
            {
                var simulationEvaluator = SourceEvaluator.Clone(true);
                simulationEvaluator.Name = simulation.Name.ToString();
                simulationEvaluator.Context = simulation;
                simulationEvaluator.Seed = SourceEvaluator.Seed;

                Evaluators[simulation] = simulationEvaluator;
            }

            return Evaluators[simulation];
        }

        public IEvaluator GetSimulationEntityEvaluator(Simulation simulation, string entityName)
        {
            var dotIndex = entityName.LastIndexOf('.');
            var simulationEvaluator = this.GetSimulationEvaluator(simulation);

            if (dotIndex == -1)
            {
                return simulationEvaluator;
            }

            string subcircuitName = entityName.Substring(0, dotIndex);
            return simulationEvaluator.FindChildEvaluator(subcircuitName);
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
