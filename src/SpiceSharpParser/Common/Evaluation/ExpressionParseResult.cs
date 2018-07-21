using System;
using System.Collections.ObjectModel;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Result of parsing expression.
    /// </summary>
    public class ExpressionParseResult
    {
        /// <summary>
        /// Gets or sets value function.
        /// </summary>
        public Func<double> Value { get; set; }

        /// <summary>
        /// Gets or sets found parameters in expression.
        /// </summary>
        public Collection<string> FoundParameters { get; set; }
    }
}
