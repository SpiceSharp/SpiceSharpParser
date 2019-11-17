using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public interface IEvaluatorConsumer
    {
        IEvaluator Evaluator { get; set; }

        ExpressionContext ExpressionContext { get; set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { get; set; }
    }
}
