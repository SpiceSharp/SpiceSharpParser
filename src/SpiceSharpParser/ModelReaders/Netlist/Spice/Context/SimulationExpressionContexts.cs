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
            this.SourceContext = sourceContext;
        }

        public int? Seed
        {
            set
            {
                this.SourceContext.Seed = value;

                foreach (var context in this.Contexts.Values)
                {
                    context.Seed = value;
                }
            }
        }

        protected ExpressionContext SourceContext { get; }

        protected ConcurrentDictionary<Simulation, ExpressionContext> Contexts = new ConcurrentDictionary<Simulation, ExpressionContext>();

        public ExpressionContext GetContext(Simulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            if (!this.Contexts.TryGetValue(simulation, out var context))
            {
                context = this.SourceContext.Clone();
                context.Data = simulation;
                Contexts[simulation] = context;
            }

            return context;
        }
    }
}
