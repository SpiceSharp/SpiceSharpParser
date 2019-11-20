using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public interface IEvaluatorConsumer
    {
        ExpressionContext ExpressionContext { get; set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { get; set; }
    }
}
