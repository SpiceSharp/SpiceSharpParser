using SpiceSharpParser.Common;

namespace SpiceSharpParser.Common
{
    public interface IEvaluatorConsumer
    {
        IEvaluator Evaluator { set; }
    }
}
