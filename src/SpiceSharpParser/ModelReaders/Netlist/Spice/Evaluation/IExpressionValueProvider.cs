using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IExpressionValueProvider
    {
        double GetExpressionValue(string expression, EvaluationContext context, bool @throw = true);
    }
}
