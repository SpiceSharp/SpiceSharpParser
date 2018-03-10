using System;
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
            ExpressionParser.Parameters = Parameters;
            Registry = new List<Tuple<SpiceSharp.Parameter, string>>();
        }

        /// <summary>
        /// Gets the dictionary of parameters values
        /// </summary>
        public Dictionary<string, double> Parameters { get; }

        /// <summary>
        /// Gets the expression parser
        /// </summary>
        protected SpiceExpression ExpressionParser { get; }


        protected List<Tuple<SpiceSharp.Parameter, string>> Registry { get; }

        /// <summary>
        /// Evalues a specific string to double
        /// </summary>
        /// <param name="value">A string to evaluate</param>
        /// <returns>
        /// A double value
        /// </returns>
        public double EvaluteDouble(string value, SpiceSharp.Parameter parameter = null)
        {
            if (Parameters.ContainsKey(value))
            {
                return Parameters[value];
            }

            if (value.StartsWith("{", System.StringComparison.Ordinal) && value.EndsWith("}", System.StringComparison.Ordinal))
            {
                value = value.Substring(1, value.Length - 2);
            }

            Registry.Add(new Tuple<SpiceSharp.Parameter, string>(parameter, value));

            return ExpressionParser.Parse(value);
        }

        public void Refresh()
        {
            foreach (var parameter in Registry)
            {
                if (parameter.Item1 != null)
                {
                    parameter.Item1.Set(ExpressionParser.Parse(parameter.Item2));
                }
            }
        }
    }
}
