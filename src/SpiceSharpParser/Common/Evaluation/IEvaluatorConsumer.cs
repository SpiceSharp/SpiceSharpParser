using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IEvaluatorConsumer
    {
        IEvaluator Evaluator { get; set; }

        ExpressionContext ExpressionContext { get; set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { set; }

        IExpressionParser ExpressionParser { set; }
    }
}
