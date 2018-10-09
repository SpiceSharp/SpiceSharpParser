using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IEvaluatorConsumer
    {
        IEvaluatorsContainer Evaluators { set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { set; }
    }
}
