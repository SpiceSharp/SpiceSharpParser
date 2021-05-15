using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public interface IEvaluatorConsumer
    {
        EvaluationContext EvaluationContext { get; set; }

        ISpiceNetlistCaseSensitivitySettings CaseSettings { get; set; }
    }
}