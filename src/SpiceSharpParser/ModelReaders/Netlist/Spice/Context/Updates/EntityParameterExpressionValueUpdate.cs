using System;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityParameterExpressionValueUpdate : EntityParameterUpdate
    { 
        public Expression Expression { get; set; }

        public override double GetValue(ExpressionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Evaluate(Expression);
        }
    }
}
