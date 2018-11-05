using System;
using System.Collections.Generic;
using System.Threading;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationExpressionContexts
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public SimulationExpressionContexts(ExpressionContext sourceContext)
        {
            SourceContext = sourceContext;
            Contexts = new Dictionary<Simulation, ExpressionContext>();
        }

        protected ExpressionContext SourceContext { get; }

        protected Dictionary<Simulation, ExpressionContext> Contexts { get; }

        /// <summary>
        /// Gets the expression context for simulation.
        /// </summary>
        /// <param name="simulation">A simulation.</param>
        /// <returns>
        /// Expression context.
        /// </returns>
        public ExpressionContext GetContext(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (!Contexts.TryGetValue(simulation, out var context))
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        context = SourceContext.Clone();
                        context.Data = simulation;
                        context.Seed = SourceContext.Seed + Contexts.Count; //TODO: better hack
                        Contexts[simulation] = context;
                        return context;
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    return context;
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}
