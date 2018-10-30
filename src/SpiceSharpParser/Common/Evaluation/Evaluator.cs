using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Evaluation.Functions;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Abstract evaluator.
    /// </summary>
    public abstract class Evaluator : IEvaluator
    {
        private int? _seed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="name">Evaluator name.</param>
        /// <param name="context">Evaluator context.</param>
        /// <param name="parser">Expression parser.</param>
        /// <param name="registry">Expression registry.</param>
        /// <param name="seed">Random seed.</param>
        /// <param name="isFunctionNameCaseSensitive">Is function name case-sensitive.</param>
        /// <param name="isParameterNameCaseSensitive">Is parameter name case-sensitive.</param>
        public Evaluator(string name, object context, IExpressionParser parser, int? seed, ExpressionRegistry registry, bool isFunctionNameCaseSensitive, bool isParameterNameCaseSensitive)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Context = context;
            ExpressionParser = parser ?? throw new ArgumentNullException(nameof(parser));
            Seed = seed;
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            IsFunctionNameCaseSensitive = isFunctionNameCaseSensitive;
            IsParameterNameCaseSensitive = isParameterNameCaseSensitive;
            Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(isParameterNameCaseSensitive));
            Functions = new Dictionary<string, Function>(StringComparerProvider.Get(isFunctionNameCaseSensitive));
            CreateCommonFunctions();
        }

        /// <summary>
        /// Gets or sets the name of the evaluator.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, Expression> Parameters { get; protected set; }

        /// <summary>
        /// Gets or sets custom functions.
        /// </summary>
        public Dictionary<string, Function> Functions { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether function names are case-sensitive.
        /// </summary>
        public bool IsFunctionNameCaseSensitive { get; }

        /// <summary>
        /// Gets a value indicating whether parameter names are case-sensitive.
        /// </summary>
        public bool IsParameterNameCaseSensitive { get; }

        /// <summary>
        /// Gets the children simulationEvaluators.
        /// </summary>
        public List<IEvaluator> Children { get; } = new List<IEvaluator>();

        /// <summary>
        /// Gets or sets the random seed for the evaluator.
        /// </summary>
        public int? Seed
        {
            get => _seed;

            set
            {
                _seed = value;

                foreach (var child in Children)
                {
                    child.Seed = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the context of the evaluator.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets the expression registry.
        /// </summary>
        public ExpressionRegistry Registry { get; }

        /// <summary>
        /// Gets the expression parser.
        /// </summary>
        protected IExpressionParser ExpressionParser { get; private set; }

        /// <summary>
        /// Evaluates a specific expression to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        public double EvaluateDouble(string expression)
        {
            if (Parameters.TryGetValue(expression, out var parameter))
            {
                return parameter.Evaluate(this);
            }

            var parseResult = ExpressionParser.Parse(expression, new ExpressionParserContext(IsFunctionNameCaseSensitive) { Functions = Functions });

            return parseResult.Value(new ExpressionEvaluationContext(IsParameterNameCaseSensitive) { Parameters = Parameters, Evaluator = this });
        }

        /// <summary>
        /// Gets the value of parameter.
        /// </summary>
        /// <param name="id">A parameter identifier.</param>
        /// <returns>
        /// A value of parameter.
        /// </returns>
        public double GetParameterValue(string id)
        {
            return Parameters[id].Evaluate(this);
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
           var parseResult = ExpressionParser.Parse(expression, new ExpressionParserContext(IsFunctionNameCaseSensitive) { Functions = Functions });
           return parseResult.FoundParameters;
        }

        /// <summary>
        /// Gets the functions from expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns>
        /// Parameters from expression.
        /// </returns>
        public ICollection<string> GetFunctionsFromExpression(string expression)
        {
            var parseResult = ExpressionParser.Parse(expression, new ExpressionParserContext(IsFunctionNameCaseSensitive) { Functions = Functions });
            return parseResult.FoundFunctions;
        }

        /// <summary>
        /// Gets a value indicating whether an expression has a constant value.
        /// </summary>
        /// <param name="expression">An expression.</param>
        /// <returns>
        /// A value indicating whether an expression has a constant value.
        /// </returns>
        public bool IsConstantExpression(string expression)
        {
            var result = ExpressionParser.Parse(expression, new ExpressionParserContext() { Functions = Functions });
            return result.FoundFunctions.Count == 0 && result.FoundParameters.Count == 0;
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, double value)
        {
            Parameters[parameterName] = new ConstantExpression(value);

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
            Parameters[parameterName] = new CachedExpression(expression);

            Registry.UpdateParameterDependencies(parameterName, GetParametersFromExpression(expression));
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
        /// Gets value of named expression.
        /// </summary>
        /// <param name="expressionName">Name of expression</param>
        /// <returns>
        /// Value of expression.
        /// </returns>
        public double GetExpressionValue(string expressionName)
        {
            return Registry.GetExpression(expressionName).Evaluate(this);
        }

        /// <summary>
        /// Sets the named expression.
        /// </summary>
        /// <param name="expressionName">Expression name.</param>
        /// <param name="expression">Expression.</param>
        public void SetNamedExpression(string expressionName, string expression)
        {
            var parameters = GetParametersFromExpression(expression);
            Registry.Add(new NamedExpression(expressionName, expression), parameters);
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
            return Registry.GetExpression(expressionName)?.String;
        }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A new evaluator.
        /// </returns>
        public abstract IEvaluator CreateChildEvaluator(string name, object context);

        /// <summary>
        /// Clones the evaluator.
        /// </summary>
        /// <param name="deep">Specifies whether cloning is deep.</param>
        /// <returns>
        /// A clone of evaluator.
        /// </returns>
        public abstract IEvaluator Clone(bool deep);

        /// <summary>
        /// Initializes the evaluator.
        /// </summary>
        /// <param name="parameters">Parameters to use.</param>
        /// <param name="functions">Functions to use.</param>
        /// <param name="children">Child evaluator to use.</param>
        public void Initialize(
            Dictionary<string, Expression> parameters, 
            Dictionary<string, Function> functions,
            List<IEvaluator> children)
        {
            foreach (var parameterName in parameters.Keys)
            {
                var parameter = parameters[parameterName].Clone();
                parameter.Invalidate();
                Parameters[parameterName] = parameter;
            }

            foreach (var customFunction in functions.Keys)
            {
                Functions[customFunction] = functions[customFunction];
            }

            Children.Clear();
            foreach (var child in children)
            {
                Children.Add(child.Clone(true));
            }

            Registry.Invalidate();
        }

        /// <summary>
        /// Refreshes parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        public void RefreshForParameter(string parameterName)
        {
            Registry.InvalidateDependentParameters(parameterName);
            Registry.InvalidateExpressions(parameterName);
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
            if (evaluatorName == Name)
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

        private void CreateCommonFunctions()
        {
            Functions.Add("acos", MathFunctions.CreateACos());
            Functions.Add("asin", MathFunctions.CreateASin());
            Functions.Add("atan", MathFunctions.CreateATan());
            Functions.Add("atan2", MathFunctions.CreateATan2());
            Functions.Add("cos", MathFunctions.CreateCos());
            Functions.Add("cosh", MathFunctions.CreateCosh());
            Functions.Add("sin", MathFunctions.CreateSin());
            Functions.Add("sinh", MathFunctions.CreateSinh());
            Functions.Add("tan", MathFunctions.CreateTan());
            Functions.Add("tanh", MathFunctions.CreateTanh());
        }
    }
}     
