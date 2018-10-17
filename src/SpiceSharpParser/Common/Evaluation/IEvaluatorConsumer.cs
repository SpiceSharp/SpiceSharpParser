using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IEvaluatorConsumer
    {
        ISimulationEvaluatorsContainer Evaluators { set; }

        SpiceNetlistCaseSensitivitySettings CaseSettings { set; }
    }
}
