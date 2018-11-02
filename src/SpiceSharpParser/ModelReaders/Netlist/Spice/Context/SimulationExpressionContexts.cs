using System;
using System.Collections.Concurrent;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class SimulationExpressionContexts
    {
        public SimulationExpressionContexts(ExpressionContext sourceContext)
        {
            SourceContext = sourceContext;
        }

        protected ExpressionContext SourceContext { get; }

        protected ConcurrentDictionary<Simulation, ExpressionContext> Contexts { get; } = new ConcurrentDictionary<Simulation, ExpressionContext>();

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

            if (!this.Contexts.TryGetValue(simulation, out var context))
            {
                context = SourceContext.Clone();
                context.Data = simulation;
                Contexts[simulation] = context;
            }

            return context;
        }
    }
}
