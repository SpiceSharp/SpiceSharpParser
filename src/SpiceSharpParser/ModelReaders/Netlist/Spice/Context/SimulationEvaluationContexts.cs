using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationEvaluationContexts
    {
        private readonly ReaderWriterLockSlim _cacheLock = new (LockRecursionPolicy.NoRecursion);

        public SimulationEvaluationContexts(EvaluationContext sourceContext)
        {
            SourceContext = sourceContext ?? throw new ArgumentNullException(nameof(sourceContext));
            Contexts = new Dictionary<Simulation, EvaluationContext>();
        }

        protected EvaluationContext SourceContext { get; }

        protected Dictionary<Simulation, EvaluationContext> Contexts { get; }

        /// <summary>
        /// Gets the expression context for simulation.
        /// </summary>
        /// <param name="simulation">A simulation.</param>
        /// <returns>
        /// Expression context.
        /// </returns>
        public EvaluationContext GetContext(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (!Contexts.TryGetValue(simulation, out var context))
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        context = SourceContext.Clone();
                        context.Simulation = simulation;
                        context.Seed = SourceContext.Seed + Contexts.Count; // TODO: better hack
                        Contexts[simulation] = context;
                        return context;
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return context;
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}