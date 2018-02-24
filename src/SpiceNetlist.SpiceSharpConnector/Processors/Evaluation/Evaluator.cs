using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// Evalues strings to double
    /// </summary>
    public class Evaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="parameters">Available parameters values</param>
        public Evaluator(Dictionary<string, double> parameters)
        {
            Parameters = parameters;
            ExpressionParser = new SpiceExpression();
        }

        /// <summary>
        /// Gets the dictionary of parameters values
        /// </summary>
        public Dictionary<string, double> Parameters { get; }

        /// <summary>
        /// Gets the expression parser
        /// </summary>
        protected SpiceExpression ExpressionParser { get; }

        /// <summary>
        /// Evalues a specific string to double
        /// </summary>
        /// <param name="value">A string to evaluate</param>
        /// <returns>
        /// A double value
        /// </returns>
        public double EvaluteDouble(string value)
        {
            if (Parameters.ContainsKey(value))
            {
                return Parameters[value];
            }

            if (value.StartsWith("{", System.StringComparison.Ordinal) && value.EndsWith("}", System.StringComparison.Ordinal))
            {
                value = value.Substring(1, value.Length - 2);
            }

            ExpressionParser.Parameters = Parameters;

            return ExpressionParser.Parse(value);
        }
    }
}
