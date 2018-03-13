using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    /// <summary>
    /// Evalues strings to double
    /// </summary>
    public class Evaluator : IEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="parameters">Available parameters values</param>
        public Evaluator(Dictionary<string, double> parameters)
        {
            Parameters = parameters;

            ExpressionParser = new SpiceExpression
            {
                Parameters = Parameters
            };

            Registry = new ExpressionRegistry();
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
        /// Gets the evaluator registry
        /// </summary>
        protected ExpressionRegistry Registry { get; }

        /// <summary>
        /// Evalues a specific string to double
        /// </summary>
        /// <param name="expression">An expression to evaluate</param>
        /// <returns>
        /// A double value
        /// </returns>
        public double EvaluteDouble(string expression)
        {
            if (Parameters.ContainsKey(expression))
            {
                return Parameters[expression];
            }

            return ExpressionParser.Parse(expression);
        }

        /// <summary>
        /// Sets the parameter value and updates the values expressions
        /// </summary>
        /// <param name="parameterName">A name of parameter</param>
        /// <param name="value">A value of parameter</param>
        public void SetParameter(string parameterName, double value)
        {
            Parameters[parameterName] = value;
            Refresh(parameterName);
        }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change
        /// </summary>
        /// <param name="expression">An expression to add</param>
        public void AddDynamicExpression(DoubleExpression expression)
        {
            Registry.Add(expression);
        }

        /// <summary>
        /// Refreshes expressions in evaluator that contains given parameter
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        private void Refresh(string parameterName)
        {
            foreach (DoubleExpression definion in Registry.GetDependedExpressions(parameterName))
            {
                var setter = definion.Setter;
                var expression = definion.Expression;

                setter(ExpressionParser.Parse(expression));
            }
        }
    }
}
