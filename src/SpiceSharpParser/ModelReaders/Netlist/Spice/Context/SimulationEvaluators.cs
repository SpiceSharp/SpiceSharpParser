using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;
using System.Threading;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationEvaluators : ISimulationEvaluators
    {
        protected Dictionary<Simulation, IEvaluator> evaluators = new Dictionary<Simulation, IEvaluator>();
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public SimulationEvaluators(IEvaluator sourceEvaluator)
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

            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (!evaluators.TryGetValue(simulation, out var evaluator))
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        evaluator = new SpiceEvaluator(
                            simulation.Name,
                            SourceEvaluator.ExpressionParser,
                            SourceEvaluator.IsParameterNameCaseSensitive,
                            SourceEvaluator.IsFunctionNameCaseSensitive);

                        evaluators[simulation] = evaluator;

                        return evaluator;
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return evaluator;
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}
