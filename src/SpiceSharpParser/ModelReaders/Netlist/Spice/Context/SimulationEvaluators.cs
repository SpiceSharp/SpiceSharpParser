using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    using System.Collections.Concurrent;

    using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

    public class SimulationEvaluators : ISimulationEvaluators
    {
        protected ConcurrentDictionary<Simulation, IEvaluator> Evaluators = new ConcurrentDictionary<Simulation, IEvaluator>();

        public SimulationEvaluators(IEvaluator sourceEvaluator, IFunctionFactory functionFactory)
        {
            SourceEvaluator = sourceEvaluator ?? throw new ArgumentNullException(nameof(sourceEvaluator));
        }

        protected IEvaluator SourceEvaluator { get; }

        public IEvaluator GetEvaluator(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!Evaluators.TryGetValue(simulation, out var evaluator))
            {
                evaluator = new SpiceEvaluator(
                    simulation.Name,
                    SourceEvaluator.ExpressionParser,
                    SourceEvaluator.IsParameterNameCaseSensitive, 
                    SourceEvaluator.IsFunctionNameCaseSensitive);

                evaluator.Name = simulation.Name;
                Evaluators[simulation] = evaluator;
            }

            return evaluator;
        }
    }
}
