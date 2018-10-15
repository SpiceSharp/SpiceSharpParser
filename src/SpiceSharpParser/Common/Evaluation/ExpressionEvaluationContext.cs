using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionEvaluationContext
    {
        public ExpressionEvaluationContext()
            : this(false)
        {
        }

        public ExpressionEvaluationContext(bool caseSensitiveParameters)
        {
            Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(caseSensitiveParameters));
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public Dictionary<string, Expression> Parameters { get; set; }


        public IEvaluator Evaluator { get; set; }
    }
}
