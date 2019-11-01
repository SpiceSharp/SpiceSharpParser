using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class ConstantExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpression"/> class.
        /// </summary>
        /// <param name="value">Value.</param>
        public ConstantExpression(double value)
            : base(string.Empty)
        {
            CurrentValue = value;
        }

        /// <summary>
        /// Clones the named expression.
        /// </summary>
        /// <returns>
        /// A clone of named expression.
        /// </returns>
        public override Expression Clone()
        {
            return new ConstantExpression(CurrentValue);
        }

        public override void Invalidate()
        {
        }

        public override double Evaluate(IEvaluator evaluator, ExpressionContext context, Simulation sim, IReadingContext readingContext)
        {
            if (evaluator == null)
            {
                throw new ArgumentNullException(nameof(evaluator));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return CurrentValue;
        }
    }
}
