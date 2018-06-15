using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Evaluator of expressions.
    /// </summary>
    public class Evaluator : IEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="parser">Expression parser</param>
        /// <param name="registry">Expression registry</param>
        public Evaluator(IExpressionParser parser, ExpressionRegistry registry)
        {
            ExpressionParser = parser ?? throw new ArgumentNullException(nameof(parser));
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Gets the dictionary of custom functions.
        /// </summary>
        public Dictionary<string, CustomFunction> CustomFunctions => ExpressionParser.CustomFunctions;

        /// <summary>
        /// Gets the expression registry.
        /// </summary>
        protected ExpressionRegistry Registry { get; }

        /// <summary>
        /// Gets the expression parser.
        /// </summary>
        protected IExpressionParser ExpressionParser { get; private set; }

        /// <summary>
        /// Gets the dictionary of parameters.
        /// </summary>
        protected Dictionary<string, EvaluatorExpression> Parameters => ExpressionParser.Parameters;

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
                return Parameters[expression].Evaluate(context);
            }

            return ExpressionParser.Parse(expression, context, this).Value();
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
            return Parameters[parameterName].Evaluate(context);
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
        /// Gets the parameters from expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>
        /// Parameters from expression.
        /// </returns>
        public ICollection<string> GetParametersFromExpression(string expression)
        {
            var result = ExpressionParser.Parse(expression, null, this);
            return result.FoundParameters;
        }


        /// <summary>
        /// Invalidates paramters.
        /// </summary>
        public void InvalidateParameters()
        {
            var clonedParameters = new Dictionary<string, EvaluatorExpression>(Parameters);
            foreach (var parameter in clonedParameters.Values)
            {
                parameter.Invalidate();
            }
        }

        /// <summary>
        /// Sets parameters.
        /// </summary>
        /// <param name="parameters">Parameters to set.</param>
        public void SetParameters(Dictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                Parameters[parameter.Key] = new CachedExpression(
                    parameter.Value,
                    (e, c, a) =>
                    this.EvaluateDouble(e, c));

                RefreshForParameter(parameter.Key);
            }
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, double value)
        {
            Parameters[parameterName] = new CachedExpression((e, c, a) => { return value; });

            RefreshForParameter(parameterName);
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="expressionString">An expression of parameter.</param>
        public void SetParameter(string parameterName, string expressionString)
        {
            Parameters[parameterName] = new CachedExpression(expressionString, (e, c, a) => this.EvaluateDouble(e, c));

            Registry.UpdateParameterDependencies(parameterName, this.GetParametersFromExpression(expressionString));
            RefreshForParameter(parameterName);
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
        /// Adds a new custom function.
        /// </summary>
        public void AddCustomFunction(
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

                var functionBodyExpression = new EvaluatorExpression(functionBody, (expr, exprContext, expression) => childEvaluator.EvaluateDouble(expr, exprContext));

                return functionBodyExpression.Evaluate(context);
            };

            this.CustomFunctions.Add(name, userFunction);
        }

        /// <summary>
        /// Gets value of named expression.
        /// </summary>
        /// <param name="expressionName">Name of expression</param>
        /// <param name="context">Context</param>
        /// <returns>
        /// Value of expression.
        /// </returns>
        public double GetExpressionValue(string expressionName, object context)
        {
            return Registry.GetExpression(expressionName).Evaluate(context);
        }

        /// <summary>
        /// Checks whether expression exists.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <returns>
        /// True if expression exists with given name.
        /// </returns>
        public bool HasExpression(string expressionName)
        {
            return Registry.HasExpression(expressionName);
        }

        /// <summary>
        /// Sets the named expression.
        /// </summary>
        /// <param name="expressionName">Expression name.</param>
        /// <param name="expression">Expression.</param>
        public void SetNamedExpression(string expressionName, string expression)
        {
            var parameters = GetParametersFromExpression(expression);
            Registry.Add(new NamedExpression(expressionName, expression, (e, c, a) => EvaluateDouble(e, c)), parameters);
        }

        /// <summary>
        /// Gets the expression by name.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <returns>
        /// Expression.
        /// </returns>
        public string GetExpression(string expressionName)
        {
            return Registry.GetExpression(expressionName).ExpressionString;
        }

        /// <summary>
        /// Adds evaluator action.
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="expressionString">Expression</param>
        /// <param name="expressionAction">Expression action</param>
        public void AddAction(string actionName, string expressionString, Action<double> expressionAction)
        {
            var expression = new NamedExpression(actionName, expressionString, (e, c, exp) => {
                    var newValue = this.EvaluateDouble(e, c);
                    if (newValue != exp.LastValue)
                    {
                        expressionAction(newValue);
                    }
                    return newValue;
                });

            var parameters = GetParametersFromExpression(expressionString);

            Registry.Add(expression, parameters);
        }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A new evaluator.
        /// </returns>
        public virtual IEvaluator CreateChildEvaluator()
        {
            var newEvaluator = new Evaluator(ExpressionParser, Registry); // Pass new registry ?

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
        /// Refreshes parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        public void RefreshForParameter(string parameterName)
        {
            Registry.RefreshDependentParameters(parameterName, null, parameterToRefresh =>
            {
                if (parameterToRefresh != parameterName)
                {
                    Parameters[parameterToRefresh].Invalidate();
                    Parameters[parameterToRefresh].Evaluate(null);
                }
            });
        }
    }
}
