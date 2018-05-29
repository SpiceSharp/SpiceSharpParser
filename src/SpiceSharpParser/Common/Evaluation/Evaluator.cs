using System.Collections.Generic;
using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.Parser.Expressions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation
{
    /// <summary>
    /// Evaluator of expressions.
    /// </summary>
    public class Evaluator : IEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        public Evaluator(
            IExpressionParser expressionParser,
            ExpressionRegistry registry)
        {
            ExpressionParser = expressionParser;
            Parameters = expressionParser.Parameters;
            Registry = registry;
        }

        /// <summary>
        /// Gets the dictionary of parameters values.
        /// </summary>
        public Dictionary<string, double> Parameters { get; }

        /// <summary>
        /// Gets the custom functions.
        /// </summary>
        public Dictionary<string, CustomFunction> CustomFunctions => ExpressionParser.CustomFunctions;

        /// <summary>
        /// Gets the expression parser.
        /// </summary>
        protected IExpressionParser ExpressionParser { get; private set; }

        /// <summary>
        /// Gets the expression registry.
        /// </summary>
        public ExpressionRegistry Registry { get; }

        /// <summary>
        /// Evalues a specific string to double
        /// </summary>
        /// <param name="expression">An expression to evaluate</param>
        /// <param name="context">Context of expression</param>
        /// <returns>
        /// A double value
        /// </returns>
        public double EvaluateDouble(string expression, object context = null)
        {
            if (Parameters.ContainsKey(expression))
            {
                return Parameters[expression];
            }

            return ExpressionParser.Parse(expression, context);
        }

        /// <summary>
        /// Sets the parameter value and updates the values expressions.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, double value)
        {
            Parameters[parameterName] = value;
            Refresh(parameterName);
        }

        /// <summary>
        /// Sets the parameter value to the value of expression and updates the values expressions.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="expression">A value of parameter.</param>
        public void SetParameter(string parameterName, string expression)
        {
            Parameters[parameterName] = EvaluateDouble(expression);
            Refresh(parameterName);
        }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change.
        /// </summary>
        /// <param name="expression">An expression to add.</param>
        /// <param name="parameters">Parameters of expression.</param>
        public void AddDynamicExpression(DoubleExpression expression, IEnumerable<string> parameters)
        {
            Registry.Add(expression, parameters);
        }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <param name="expression">An expression to add.</param>
        /// <param name="parameters">Parameters of expression.</param>
        public void AddNamedDynamicExpression(string expressionName, DoubleExpression expression, IEnumerable<string> parameters)
        {
            Registry.Add(expressionName, expression, parameters);
        }

        /// <summary>
        /// Returns names of all parameters.
        /// </summary>
        /// <returns>
        /// True if there is parameter.
        /// </returns>
        public IEnumerable<string> GetParameters()
        {
            return Parameters.Keys;
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
        /// Gets the expression.
        /// </summary>
        /// <param name="expressionName">A expression name</param>
        /// <returns>
        /// An expression.
        /// </returns>
        public string GetExpression(string expressionName)
        {
            return Registry.GetExpression(expressionName);
        }

        /// <summary>
        /// Gets the names of parameters.
        /// </summary>
        /// <returns>
        /// The names of paramaters.
        /// </returns>
        public IEnumerable<string> GetParameterNames()
        {
            return Parameters.Keys;
        }

        /// <summary>
        /// Sets the parameters values and updates the values expressions.
        /// </summary>
        /// <param name="parameters">A dictionary of parameter values.</param>
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
        /// Gets the variables in expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>
        /// A list of variables from expression.
        /// </returns>
        public IEnumerable<string> GetVariables(string expression)
        {
            ExpressionParser.Parse(expression);

            return ExpressionParser.Variables;
        }

        /// <summary>
        /// Gets the expression names.
        /// </summary>
        /// <returns>
        /// Enumerable of strings.
        /// </returns>
        public IEnumerable<string> GetExpressionNames()
        {
            return Registry.GetExpressionNames();
        }

        /// <summary>
        /// Refreshes expressions in evaluator that contains given parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        private void Refresh(string parameterName)
        {
            foreach (DoubleExpression definion in Registry.GetDependentExpressions(parameterName))
            {
                var setter = definion.Setter;
                var expression = definion.ValueExpression;

                var newValue = ExpressionParser.Parse(expression);
                setter(newValue);
            }
        }
    }
}
