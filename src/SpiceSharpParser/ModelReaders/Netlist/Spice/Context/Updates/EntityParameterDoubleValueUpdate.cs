using System;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityParameterDoubleValueUpdate : EntityParameterUpdate
    {
        public double Value { get; set; }

        public override double GetValue(ExpressionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Value;
        }
    }
}
