using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class EntityParameterDoubleValueUpdate : EntityParameterUpdate
    {
        public double Value { get; set; }

        public override double GetValue(IEvaluator evaluator, ExpressionContext context)
        {
            return Value;
        }
    }
}
