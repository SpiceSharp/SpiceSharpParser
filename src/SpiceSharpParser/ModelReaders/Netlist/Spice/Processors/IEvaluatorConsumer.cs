using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Processors
{
    public interface IEvaluatorConsumer
    {
        EvaluationContext EvaluationContext { get; set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { get; set; }
    }
}