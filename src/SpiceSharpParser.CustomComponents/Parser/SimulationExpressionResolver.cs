using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using System;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Connects a behavioral expression to the reader's static or simulation-aware resolver.
    /// </summary>
    internal static class SimulationExpressionResolver
    {
        public static void Configure(
            IReadingContext context,
            string expression,
            Action<Func<string, Node>> setParser)
        {
            setParser(CreateParser(context, null));

            if (context.EvaluationContext.HaveFunctions(expression))
            {
                context.SimulationPreparations.ExecuteActionBeforeSetup(
                    simulation => setParser(CreateParser(context, simulation)));
            }
        }

        private static Func<string, Node> CreateParser(
            IReadingContext context,
            ISimulationWithEvents simulation)
        {
            return expression => context.CreateExpressionResolver(simulation).Resolve(expression);
        }
    }
}
