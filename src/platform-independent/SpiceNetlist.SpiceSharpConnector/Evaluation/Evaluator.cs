using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Evaluation
{
    /// <summary>
    /// Evalues strings to double
    /// </summary>
    public class Evaluator : IEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        public Evaluator()
        {
            Parameters = new Dictionary<string, double>();

            ExpressionParser = new SpiceExpression
            {
                Parameters = Parameters
            };

            Registry = new ExpressionRegistry();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="parentEvaluator">Parent evaluator</param>
        public Evaluator(IEvaluator parentEvaluator)
            : this()
        {
            foreach (var parameterName in parentEvaluator.GetParameterNames())
            {
                Parameters[parameterName] = parentEvaluator.GetParameterValue(parameterName);
            }
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
        /// <param name="expressionParameters">Found parameters in expression</param>
        /// <returns>
        /// A double value
        /// </returns>
        public double EvaluateDouble(string expression, out List<string> expressionParameters)
        {
            if (Parameters.ContainsKey(expression))
            {
                expressionParameters = new List<string>() { expression };
                return Parameters[expression];
            }

            return ExpressionParser.Parse(expression, out expressionParameters);
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
        /// Sets the parameter value to the value of expression and updates the values expressions
        /// </summary>
        /// <param name="parameterName">A name of parameter</param>
        /// <param name="expression">A value of parameter</param>
        public void SetParameter(string parameterName, string expression)
        {
            Parameters[parameterName] = EvaluateDouble(expression, out _);
            Refresh(parameterName);
        }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change
        /// </summary>
        /// <param name="expression">An expression to add</param>
        public void AddDynamicExpression(DoubleExpression expression)
        {
            EvaluateDouble(expression.ValueExpression, out var parameters);
            Registry.Add(expression, parameters);
        }

        /// <summary>
        /// Returns a value indicating whether there is a parameter in evaluator with given name
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        /// <returns>
        /// True if there is parameter
        /// </returns>
        public bool HasParameter(string parameterName)
        {
            return Parameters.ContainsKey(parameterName);
        }

        /// <summary>
        /// Gets the value of parameter
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        /// <returns>
        /// A value of parameter
        /// </returns>
        public double GetParameterValue(string parameterName)
        {
            return Parameters[parameterName];
        }

        /// <summary>
        /// Gets the names of parameters
        /// </summary>
        /// <returns>
        /// The names of paramaters
        /// </returns>
        public IEnumerable<string> GetParameterNames()
        {
            return Parameters.Keys;
        }

        /// <summary>
        /// Sets the parameters values and updates the values expressions
        /// </summary>
        /// <param name="parameters">A dictionary of parameter values</param>
        public void SetParameters(Dictionary<string, double> parameters)
        {
            foreach (var parameter in parameters)
            {
                Parameters[parameter.Key] = parameter.Value;
            }

            foreach (var parameter in parameters)
            {
                Refresh(parameter.Key);
            }
        }

        /// <summary>
        /// Refreshes expressions in evaluator that contains given parameter
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        private void Refresh(string parameterName)
        {
            foreach (DoubleExpression definion in Registry.GetDependentExpressions(parameterName))
            {
                var setter = definion.Setter;
                var expression = definion.ValueExpression;

                var newValue = ExpressionParser.Parse(expression, out _);
                setter(newValue);
            }
        }
    }
}
