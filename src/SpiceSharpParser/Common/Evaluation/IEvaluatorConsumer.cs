using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common
{
    public interface IEvaluatorConsumer
    {
        IEvaluatorsContainer Evaluators { set; }
    }
}
