using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityParameterDoubleValueUpdate : EntityParameterUpdate
    {
        public double Value { get; set; }

        public override double GetValue(EvaluationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Value;
        }
    }
}