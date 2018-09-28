using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation.Expressions;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Abstract evaluator.
    /// </summary>
    public abstract class Evaluator : IEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="name">Evaluator name.</param>
        /// <param name="context">Evaluator context.</param>
        /// <param name="parser">Expression parser.</param>
        /// <param name="registry">Expression registry.</param>
        /// <param name="seed">Random seed.</param>
        public Evaluator(string name, object context, IExpressionParser parser, ExpressionRegistry registry, int? seed)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Context = context;
            ExpressionParser = parser ?? throw new ArgumentNullException(nameof(parser));
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            Seed = seed;
        }

        /// <summary>
        /// Gets or sets the name of the evaluator.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the children evaluators.
        /// </summary>
        public List<IEvaluator> Children { get; } = new List<IEvaluator>();

        /// <summary>
        /// Gets the dictionary of custom functions.
        /// </summary>
        public Dictionary<string, CustomFunction> CustomFunctions => ExpressionParser.CustomFunctions;

        /// <summary>
        /// Gets or sets the random seed for the evaluator.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the context of the evaluator.
        /// </summary>
        public object Context { get; set; }

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
        /// <returns>
        /// A double value.
        /// </returns>
        public double EvaluateDouble(string expression)
        {
            if (Parameters.ContainsKey(expression))
            {
                return Parameters[expression].Evaluate();
            }

            var parseResult = ExpressionParser.Parse(expression, this);

            return parseResult.Value();
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
        public double GetParameterValue(string parameterName)
        {
            return Parameters[parameterName].Evaluate();
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
            var result = ExpressionParser.Parse(expression, this, false);
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
                Parameters[parameter.Key] = new CachedEvaluatorExpression(parameter.Value, this);
                Registry.UpdateParameterDependencies(parameter.Key, this.GetParametersFromExpression(parameter.Value));
            }

            foreach (var parameter in parameters)
            {
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
            Parameters[parameterName] = new ConstantEvaluatorExpression(value);

            RefreshForParameter(parameterName);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, value);
            }
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="expression">An expression of parameter.</param>
        public void SetParameter(string parameterName, string expression)
        {
            Parameters[parameterName] = new CachedEvaluatorExpression(expression, this);

            Registry.UpdateParameterDependencies(parameterName, this.GetParametersFromExpression(expression));
            RefreshForParameter(parameterName);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, expression);
            }
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

            userFunction.Logic = (args, evaluator) =>
            {
                var childEvaluator = evaluator.CreateChildEvaluator(evaluator.Name + "_" + name, evaluator.Context);
                for (var i = 0; i < arguments.Count; i++)
                {
                    childEvaluator.SetParameter(arguments[i], (double)args[i]);
                }

                var functionBodyExpression = new EvaluatorExpression(functionBody, childEvaluator);

                return functionBodyExpression.Evaluate();
            };

            this.CustomFunctions.Add(name, userFunction);
        }

        /// <summary>
        /// Gets value of named expression.
        /// </summary>
        /// <param name="expressionName">Name of expression</param>
        /// <returns>
        /// Value of expression.
        /// </returns>
        public double GetExpressionValue(string expressionName)
        {
            return Registry.GetExpression(expressionName).Evaluate();
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
            Registry.Add(new NamedEvaluatorExpression(expressionName, expression, this), parameters);
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
            return Registry.GetExpression(expressionName).Expression;
        }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A new evaluator.
        /// </returns>
        public abstract IEvaluator CreateChildEvaluator(string name, object context);

        public abstract IEvaluator Clone(bool deep);

        public void Initialize(Dictionary<string, EvaluatorExpression> parameters, Dictionary<string, CustomFunction> customFunctions, List<IEvaluator> children)
        {
            foreach (var parameterName in parameters.Keys)
            {
                this.Parameters[parameterName] = parameters[parameterName].Clone();
                this.Parameters[parameterName].Evaluator = this;
                this.Parameters[parameterName].Invalidate();
            }

            foreach (var customFunction in customFunctions.Keys)
            {
                CustomFunctions[customFunction] = customFunctions[customFunction];
            }

            Children.Clear();
            foreach (var child in children)
            {
                Children.Add(child.Clone(true));
            }

            Registry.Invalidate(this);
        }

        /// <summary>
        /// Refreshes parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        public void RefreshForParameter(string parameterName)
        {
            Registry.RefreshDependentParameters(parameterName, parameterToRefresh =>
            {
                if (parameterToRefresh != parameterName)
                {
                    Parameters[parameterToRefresh].Invalidate();
                    Parameters[parameterToRefresh].Evaluate();
                }
            });
        }

        /// <summary>
        /// Finds the child evaluator with given name.
        /// </summary>
        /// <param name="evaluatorName">Name of child evaluator to find.</param>
        /// <returns>
        /// A reference to evaluator.
        /// </returns>
        public IEvaluator FindChildEvaluator(string evaluatorName)
        {
            if (evaluatorName == this.Name)
            {
                return this;
            }

            foreach (var child in Children)
            {
                var result = child.FindChildEvaluator(evaluatorName);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds evaluator action.
        /// </summary>
        /// <param name="actionName">Action name.</param>
        /// <param name="expressionString">Expression.</param>
        /// <param name="expressionAction">Expression action.</param>
        public void AddAction(string actionName, string expressionString, Action<double> expressionAction)
        {
            if (expressionAction == null)
            {
                throw new ArgumentNullException(nameof(expressionAction));
            }

            var namedExpression = new NamedEvaluatorExpression(actionName, expressionString, this);
            namedExpression.Evaluated += (object s, EvaluatedArgs args) => { expressionAction(args.NewValue); };

            var parameters = GetParametersFromExpression(expressionString);
            Registry.Add(namedExpression, parameters);
        }
    }
}
