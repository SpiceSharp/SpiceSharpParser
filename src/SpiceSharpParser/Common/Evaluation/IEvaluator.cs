using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IEvaluator
    {
        double EvaluateDouble(string expression);
        double EvaluateDouble(Parameter parameter);
        double EvaluateDouble(Expression expression);
        
        IExpressionValueProvider ExpressionValueProvider { get; }
    }
}