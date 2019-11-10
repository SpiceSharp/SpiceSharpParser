using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// An interface for all evaluators.
    /// </summary>
    public interface IEvaluator
    {
        double Evaluate(Expression expression, ExpressionContext context, Simulation simulation, IReadingContext readingContext);
    }
}
