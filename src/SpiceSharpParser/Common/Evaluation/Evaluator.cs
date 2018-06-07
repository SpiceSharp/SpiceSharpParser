using System.Collections.Generic;
using System.Globalization;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation
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
        /// Gets the custom functions.
        /// </summary>
        public Dictionary<string, CustomFunction> CustomFunctions => ExpressionParser.CustomFunctions;

        /// <summary>
        /// Gets the expression parser.
        /// </summary>
        protected IExpressionParser ExpressionParser { get; private set; }

        /// <summary>
        /// Gets the dictionary of parameters expressions.
        /// </summary>
        protected Dictionary<string, LazyExpression> Parameters { get; }

        /// <summary>
        /// Gets the expression registry.
        /// </summary>
        protected ExpressionRegistry Registry { get; }

        /// <summary>
        /// Evalues a specific string to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <param name="context">Context of expression.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        public double EvaluateDouble(string expression, object context = null)
        {
            if (Parameters.ContainsKey(expression))
            {
                return Parameters[expression].GetValue(context);
            }

            return ExpressionParser.Parse(expression, context, this).Value();
        }

        /// <summary>
        /// Sets the parameter value and updates the values expressions.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, double value)
        {
            Parameters[parameterName] = new LazyExpression((e, c) => value, value.ToString(CultureInfo.InvariantCulture));
            Refresh(parameterName);
        }

        /// <summary>
        /// Sets the parameter value to the value of expression and updates the values expressions.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="expression">A value of parameter.</param>
        public void SetParameter(string parameterName, string expression)
        {
            Parameters[parameterName] = new LazyExpression((e,c) => this.EvaluateDouble(e,c), expression);
            Refresh(parameterName);
        }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change.
        /// </summary>
        /// <param name="expression">An expression to add.</param>
        /// <param name="parameters">Parameters of expression.</param>
        public void AddActionExpression(ActionExpression expression, IEnumerable<string> parameters)
        {
            Registry.Add(expression, parameters);
        }

        /// <summary>
        /// Adds double expression to registry that will be updated when value of parameter change.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <param name="expression">An expression to add.</param>
        /// <param name="parameters">Parameters of expression.</param>
        public void AddNamedActionExpression(string expressionName, ActionExpression expression, IEnumerable<string> parameters)
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
        /// Returns a value indicating whether there is a parameter in evaluator with given name.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// True if there is parameter.
        /// </returns>
        public bool HasParameter(string parameterName)
        {
            return Parameters.ContainsKey(parameterName);
        }

        /// <summary>
        /// Gets the value of parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// A value of parameter.
        /// </returns>
        public double GetParameterValue(string parameterName, object context)
        {
            return Parameters[parameterName].GetValue(context);
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
        public void SetParameters(Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                Parameters[parameter.Key] = new LazyExpression((e, c) => this.EvaluateDouble(e, c), parameter.Value);
            }

            foreach (var parameter in parameters)
            {
                Refresh(parameter.Key);
            }
        }

        /// <summary>
        /// Gets the parameters from expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>
        /// Parameters from expression.
        /// </returns>
        public IEnumerable<string> GetParametersFromExpression(string expression)
        {
            var result = ExpressionParser.Parse(expression, null, this);
            return result.FoundParameters;
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
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A new evaluator.
        /// </returns>
        public virtual IEvaluator CreateChildEvaluator()
        {
            var newEvaluator = new Evaluator(ExpressionParser, Registry);

            foreach (var parameterName in this.GetParameterNames())
            {
                newEvaluator.Parameters[parameterName] = this.ExpressionParser.Parameters[parameterName];
            }

            foreach (var customFunction in CustomFunctions)
            {
                newEvaluator.CustomFunctions[customFunction.Key] = customFunction.Value;
            }

            return newEvaluator;
        }

        /// <summary>
        /// Defines a new custom function.
        /// </summary>
        public void DefineCustomFunction(
            string name,
            List<string> arguments,
            string functionBody)
        {
            CustomFunction userFunction = new CustomFunction();
            userFunction.Name = name;
            userFunction.VirtualParameters = false;
            userFunction.ArgumentsCount = arguments.Count;

            userFunction.Logic = (args, context, evaluator) =>
            {
                var childEvaluator = evaluator.CreateChildEvaluator();
                for (var i = 0; i < arguments.Count; i++)
                {
                    childEvaluator.SetParameter(arguments[i], (double)args[i]);
                }

                return childEvaluator.EvaluateDouble(functionBody, context);
            };

            this.CustomFunctions.Add(name, userFunction);
        }

        /// <summary>
        /// Invalidates paramters.
        /// </summary>
        public void InvalidateParameters()
        {
            var clonedParameters = new Dictionary<string, LazyExpression>(Parameters);
            foreach (var parameter in clonedParameters)
            {
                parameter.Value.Invalidate();
            }

            foreach (var parameter in clonedParameters)
            {
                Refresh(parameter.Key);
            }
        }

        /// <summary>
        /// Refreshes expressions in evaluator that contains given parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        private void Refresh(string parameterName)
        {
            foreach (ActionExpression actionExpression in Registry.GetDependentExpressions(parameterName))
            {
                var newValue = ExpressionParser.Parse(actionExpression.Expression, null, this).Value();
                actionExpression.Action(newValue);
            }
        }
    }
}
