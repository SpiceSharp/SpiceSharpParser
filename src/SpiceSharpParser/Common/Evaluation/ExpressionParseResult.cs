using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Expression parse result.
    /// </summary>
    public class ExpressionParseResult
    {
        /// <summary>
        /// Gets or sets value function.
        /// </summary>
        public Func<ExpressionEvaluationContext, double> Value { get; set; }

        /// <summary>
        /// Gets or sets found parameters in expression.
        /// </summary>
        public Collection<string> FoundParameters { get; set; }

        /// <summary>
        /// Gets or sets found functions in expression.
        /// </summary>
        public Collection<string> FoundFunctions { get; set; }

        /// <summary>
        /// Gets a value indicating whether the expression is constant.
        /// </summary>
        public bool IsConstantExpression => !FoundFunctions.Any() && !FoundParameters.Any();
    }
}
