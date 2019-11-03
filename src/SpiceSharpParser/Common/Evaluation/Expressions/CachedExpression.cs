using System;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation.Expressions
{
    public class CachedExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedExpression"/> class.
        /// </summary>
        /// <param name="expression">Expression string.</param>
        public CachedExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Gets a value indicating whether value of cached expression has been computed.
        /// </summary>
        protected bool IsEvaluated { get; private set; }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="evaluator">Evaluator.</param>
        /// <param name="context">Context.</param>
        /// <param name="sim">Simulation.</param>
        /// <param name="readingContext">Reading context.</param>
        /// <returns>
        /// The value of the expression.
        /// </returns>
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

            if (!IsEvaluated)
            {
                CurrentValue = evaluator.EvaluateValueExpression(ValueExpression,context, sim, readingContext);
                IsEvaluated = true;
            }

            return CurrentValue;
        }

        /// <summary>
        /// Invalidate the expression.
        /// </summary>
        public override void Invalidate()
        {
            IsEvaluated = false;
        }

        /// <summary>
        /// Clones the cached expression.
        /// </summary>
        /// <returns>
        /// A cloned cached expression.
        /// </returns>
        public override Expression Clone()
        {
            return new CachedExpression(ValueExpression) { CurrentValue = CurrentValue, IsEvaluated = true };
        }
    }
}
