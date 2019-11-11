using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates
{
    public class EntityParameterExpressionValueUpdate : EntityParameterUpdate
    {
        private readonly IReadingContext _readingContext;
        public Expression Expression { get; set; }

        public EntityParameterExpressionValueUpdate(IReadingContext readingContext)
        {
            _readingContext = readingContext;
        }


        public override double GetValue(IEvaluator evaluator, ExpressionContext context, Simulation simulation)
        {
            if (evaluator == null)
            {
                throw new ArgumentNullException(nameof(evaluator));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return evaluator.Evaluate(Expression, context, simulation, _readingContext);
        }
    }
}
