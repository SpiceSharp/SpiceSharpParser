using System;
using System.Collections.Generic;
using System.Threading;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationEvaluators : ISimulationEvaluators
    {
        private readonly Dictionary<Simulation, IEvaluator> _evaluators = new Dictionary<Simulation, IEvaluator>();
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

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

            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (!_evaluators.TryGetValue(simulation, out var evaluator))
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        evaluator = new Evaluator(simulation.Name);
                        _evaluators[simulation] = evaluator;

                        return evaluator;
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return evaluator;
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}
