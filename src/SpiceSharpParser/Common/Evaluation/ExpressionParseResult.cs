using System;
using System.Collections.ObjectModel;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// SpiceSharpModel of parsing expression.
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
    }
}
