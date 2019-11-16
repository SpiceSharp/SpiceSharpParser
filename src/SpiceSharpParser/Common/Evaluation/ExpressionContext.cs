using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Evaluation.Functions;
using SpiceSharpParser.Common.Mathematics.Probability;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionContext
    {
        private readonly bool _isParameterNameCaseSensitive;
        private readonly bool _isFunctionNameCaseSensitive;
        private readonly bool _isExpressionNameCaseSensitive;
        private int? _seed;
        private object _data;

        public ExpressionContext()
            : this(string.Empty, false, false, false, new Randomizer())
        {
        }

        public ExpressionContext(string name, bool isParameterNameCaseSensitive, bool isFunctionNameCaseSensitive, bool isExpressionNameCaseSensitive, IRandomizer randomizer)
        {
            _isParameterNameCaseSensitive = isParameterNameCaseSensitive;
            _isFunctionNameCaseSensitive = isFunctionNameCaseSensitive;
            _isExpressionNameCaseSensitive = isExpressionNameCaseSensitive;

            Name = name;
            Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(isParameterNameCaseSensitive));
            Arguments = new Dictionary<string, Expression>(StringComparerProvider.Get(isParameterNameCaseSensitive));
            Functions = new Dictionary<string, List<IFunction>>(StringComparerProvider.Get(isFunctionNameCaseSensitive));
            Children = new List<ExpressionContext>();
            ExpressionRegistry = new ExpressionRegistry(isParameterNameCaseSensitive, isExpressionNameCaseSensitive);

            Randomizer = randomizer;
        }

        /// <summary>
        /// Gets the name of the context.
        /// </summary>
        public string Name { get; }

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

        public IRandomizer Randomizer { get; set; }

        /// <summary>
        /// Gets or sets data of the context.
        /// </summary>
        public object Data
        {
            get => _data;

            set
            {
                _data = value;

                foreach (var child in Children)
                {
                    child.Data = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, Expression> Parameters { get; protected set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, Expression> Arguments { get; protected set; }

        /// <summary>
        /// Gets or sets custom functions.
        /// </summary>
        public Dictionary<string, List<IFunction>> Functions { get; protected set; }

        /// <summary>
        /// Gets or sets expression registry for the context.
        /// </summary>
        public ExpressionRegistry ExpressionRegistry { get; set; }

        /// <summary>
        /// Gets or sets the children simulationEvaluators.
        /// </summary>
        public List<ExpressionContext> Children { get; set; }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, double value)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            var parameter = new ConstantExpression(value);
            Parameters[parameterName] = parameter;

            ExpressionRegistry.AddOrUpdate(parameterName, parameter);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, value);
            }
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, Func<double> value)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            var parameter = new FunctionExpression(value);
            Parameters[parameterName] = parameter;

            ExpressionRegistry.AddOrUpdate(parameterName, parameter);

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
        /// <param name="expressionParameters">Parameters in expression.</param>
        public void SetParameter(string parameterName, string expression, ICollection<string> expressionParameters)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expressionParameters == null)
            {
                throw new ArgumentNullException(nameof(expressionParameters));
            }

            var parameter = new DynamicExpression(expression);
            SetParameter(parameterName, expression, expressionParameters, parameter);
        }

        /// <summary>
        /// Gets the expression names.
        /// </summary>
        /// <returns>
        /// Enumerable of strings.
        /// </returns>
        public IEnumerable<string> GetExpressionNames()
        {
            return ExpressionRegistry.GetExpressionNames();
        }

        /// <summary>
        /// Sets the named expression.
        /// </summary>
        /// <param name="expressionName">Expression name.</param>
        /// <param name="expression">Expression.</param>
        /// <param name="parameters">Parameters.</param>
        public void SetNamedExpression(string expressionName, string expression, ICollection<string> parameters)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            ExpressionRegistry.Add(new NamedExpression(expressionName, expression), parameters);
        }

        /// <summary>
        /// Gets the expression by name.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <returns>
        /// Expression.
        /// </returns>
        public Expression GetExpression(string expressionName)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            return ExpressionRegistry.GetExpression(expressionName);
        }

        /// <summary>
        /// Creates a child context.
        /// </summary>
        /// <param name="name">Name of a context.</param>
        /// <param name="addToChildren">Specifies whether context should be added to children.</param>
        /// <returns>
        /// A child context.
        /// </returns>
        public virtual ExpressionContext CreateChildContext(string name, bool addToChildren)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            var child = new ExpressionContext(name, _isParameterNameCaseSensitive, _isFunctionNameCaseSensitive, _isExpressionNameCaseSensitive, Randomizer);

            child.Parameters = new Dictionary<string, Expression>(Parameters, StringComparerProvider.Get(_isParameterNameCaseSensitive));
            child.Data = Data;
            child.Functions = new Dictionary<string, List<IFunction>>(Functions, StringComparerProvider.Get(_isFunctionNameCaseSensitive));
            child.ExpressionRegistry = ExpressionRegistry.Clone();
            child.Seed = Seed;
            child.Randomizer = Randomizer;

            if (addToChildren)
            {
                Children.Add(child);
            }

            return child;
        }

        public virtual ExpressionContext Clone()
        {
            ExpressionContext context = new ExpressionContext(
                Name,
                _isParameterNameCaseSensitive,
                _isFunctionNameCaseSensitive,
                _isExpressionNameCaseSensitive,
                Randomizer);
            context.ExpressionRegistry = ExpressionRegistry.Clone();
            context.Functions = new Dictionary<string, List<IFunction>>(Functions, StringComparerProvider.Get(_isFunctionNameCaseSensitive));

            foreach (var parameter in Parameters)
            {
                context.Parameters.Add(parameter.Key, parameter.Value.Clone());
            }

            foreach (var child in Children)
            {
                context.Children.Add(child.Clone());
            }

            context.Seed = Seed;
            context.Data = Data;
            context.Randomizer = Randomizer;

            return context;
        }

        public void SetParameters(
            Dictionary<string, string> parameters,
            Dictionary<string, ICollection<string>> parametersOfParameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parametersOfParameters == null)
            {
                throw new ArgumentNullException(nameof(parametersOfParameters));
            }

            foreach (var paramName in parameters)
            {
                SetParameter(paramName.Key, paramName.Value, parametersOfParameters[paramName.Key]);
            }
        }

        public ExpressionContext Find(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (Name == name)
            {
                return this;
            }

            foreach (var child in Children)
            {
                var res = child.Find(name);

                if (res != null)
                {
                    return res;
                }
            }

            return null;
        }

        public void AddFunction(string name, IFunction function)
        {
            if (!Functions.ContainsKey(name))
            {
                Functions[name] = new List<IFunction>();
            }

            var overridenFunction = Functions[name].SingleOrDefault(f => f.ArgumentsCount == function.ArgumentsCount);

            if (overridenFunction != null)
            {
                Functions[name].Remove(overridenFunction);
            }

            Functions[name].Add(function);
        }

        public void CreateCommonFunctions()
        {
            AddFunction("atan2", MathFunctions.CreateATan2());
            AddFunction("cosh", MathFunctions.CreateCosh());
            AddFunction("sinh", MathFunctions.CreateSinh());
            AddFunction("tanh", MathFunctions.CreateTanh());
        }

        protected void SetParameter(string parameterName, string expression, ICollection<string> expressionParameters, Expression parameter)
        {
            Parameters[parameterName] = parameter;

            ExpressionRegistry.AddOrUpdate(parameterName, parameter);
            ExpressionRegistry.AddOrUpdateParameterDependencies(parameterName, expressionParameters);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, expression, expressionParameters);
            }
        }
    }
}
