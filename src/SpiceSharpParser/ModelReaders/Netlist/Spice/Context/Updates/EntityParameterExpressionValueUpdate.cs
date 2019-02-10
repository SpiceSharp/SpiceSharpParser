using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class EntityParameterExpressionValueUpdate : EntityParameterUpdate
    {
        public string ValueExpression { get; set; }

        public override double GetValue(IEvaluator evaluator, ExpressionContext context)
        {
            return evaluator.EvaluateValueExpression(ValueExpression, context);
        }
    }
}
