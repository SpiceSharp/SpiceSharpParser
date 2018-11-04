using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;

namespace SpiceSharpParser.Common.Processors
{
    public interface IEvaluatorConsumer
    {
        IEvaluator Evaluator { get; set; }

        ExpressionContext ExpressionContext { get; set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { set; }

        IExpressionParser ExpressionParser { set; }
    }
}
