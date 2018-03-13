using System;
using System.Collections.Generic;
using SpiceSharp;

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
            ExpressionParser.Parameters = Parameters;
            Registry = new List<Tuple<Action<double>, string, string>>();
        }

        /// <summary>
        /// Gets the dictionary of parameters values
        /// </summary>
        public Dictionary<string, double> Parameters { get; }

        /// <summary>
        /// Gets the expression parser
        /// </summary>
        protected SpiceExpression ExpressionParser { get; }

        protected List<Tuple<Action<double>, string, string>> Registry { get; }

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

            value = Strip(value);

            return ExpressionParser.Parse(value);
        }

        private static string Strip(string value)
        {
            if (value.StartsWith("{", System.StringComparison.Ordinal) && value.EndsWith("}", System.StringComparison.Ordinal))
            {
                value = value.Substring(1, value.Length - 2);
            }

            return value;
        }

        public void Refresh()
        {
            foreach (var parameter in Registry)
            {
                if (parameter.Item1 != null)
                {
                    parameter.Item1(ExpressionParser.Parse(Strip(parameter.Item2)));
                }
            }
        }

        internal void EnableRefresh(string name, Action<double> setter, string value)
        {
            if (setter != null)
            {
                Registry.Add(new Tuple<Action<double>, string, string>(setter, value, name));
            }
        }
    }
}
