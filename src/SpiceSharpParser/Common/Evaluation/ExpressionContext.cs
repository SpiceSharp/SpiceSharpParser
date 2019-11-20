using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Evaluation.Functions;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionContext
    {
        private readonly ISpiceNetlistCaseSensitivitySettings _caseSettings;
        
        public ExpressionContext(
            string name, 
            ISpiceNetlistCaseSensitivitySettings caseSettings, 
            IRandomizer randomizer, 
            IExpressionParser parser,
            INameGenerator nameGenerator)
        {
            _caseSettings = caseSettings;
            Parser = parser;
            NameGenerator = nameGenerator;
            Name = name;
            Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(caseSettings.IsParameterNameCaseSensitive));
            Arguments = new Dictionary<string, Expression>(StringComparerProvider.Get(caseSettings.IsParameterNameCaseSensitive));
            Functions = new Dictionary<string, List<IFunction>>(StringComparerProvider.Get(caseSettings.IsFunctionNameCaseSensitive));
            Children = new List<ExpressionContext>();
            ExpressionRegistry = new ExpressionRegistry(caseSettings.IsParameterNameCaseSensitive, caseSettings.IsExpressionNameCaseSensitive);

            Randomizer = randomizer;
        }

        /// <summary>
        /// Gets the name of the context.
        /// </summary>
        public string Name { get; }

        protected IExpressionParser Parser { get; }

        /// <summary>
        /// Gets or sets the random seed for the evaluator.
        /// </summary>
        public int? Seed
        {
            get => Randomizer.Seed;

            set
            {
                Randomizer.Seed = value;

                foreach (var child in Children)
                {
                    child.Seed = value;
                }
            }
        }

        public IRandomizer Randomizer { get; set; }

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

        public Simulation Simulation { get; set; }

        public INameGenerator NameGenerator { get; set; }

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
        public void SetParameter(string parameterName, string expression)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }


            var parameter = new DynamicExpression(expression);
            SetParameter(parameterName, expression, parameter);
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
        public void SetNamedExpression(string expressionName, string expression)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var parameters = Parser.GetExpressionParameters(expression, this, false);
         
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

            var child = new ExpressionContext(name, _caseSettings, Randomizer, Parser, NameGenerator);

            child.Parameters = new Dictionary<string, Expression>(Parameters, StringComparerProvider.Get(_caseSettings.IsParameterNameCaseSensitive));
            child.Simulation = Simulation;
            child.Functions = new Dictionary<string, List<IFunction>>(Functions, StringComparerProvider.Get(_caseSettings.IsFunctionNameCaseSensitive));
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
            ExpressionContext context = new ExpressionContext(Name, _caseSettings, Randomizer, Parser, NameGenerator);
            context.ExpressionRegistry = ExpressionRegistry.Clone();
            context.Functions = new Dictionary<string, List<IFunction>>(Functions, StringComparerProvider.Get(_caseSettings.IsFunctionNameCaseSensitive));

            foreach (var parameter in Parameters)
            {
                context.Parameters.Add(parameter.Key, parameter.Value.Clone());
            }

            foreach (var child in Children)
            {
                context.Children.Add(child.Clone());
            }

            context.Seed = Seed;
            context.Simulation = Simulation;
            context.Randomizer = Randomizer;

            return context;
        }

        public void SetParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (var paramName in parameters)
            {
                SetParameter(paramName.Key, paramName.Value);
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

        protected void SetParameter(string parameterName, string expression, Expression parameter)
        {
            Parameters[parameterName] = parameter;

            ExpressionRegistry.AddOrUpdate(parameterName, parameter);

            var expressionParameters = Parser.GetExpressionParameters(expression, this, false);
            ExpressionRegistry.AddOrUpdateParameterDependencies(parameterName, expressionParameters);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, expression);
            }
        }

        /// <summary>
        /// Evaluates a specific expression to double.
        /// </summary>
        /// <param name="expression">An expression to evaluate.</param>
        /// <returns>
        /// A double value.
        /// </returns>
        public virtual double Evaluate(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return Parser.GetExpressionValue(expression, this, true);
        }

        public virtual double Evaluate(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (expression.CanProvideValueDirectly)
            {
                return expression.GetValue();
            }

            return Evaluate(expression.ValueExpression);
        }

        public bool HaveSpiceProperties(string expression)
        {
            return Parser.HaveSpiceProperties(expression, this);
        }

        public bool HaveFunctions(string expression)
        {
            return Parser.HaveFunctions(expression, this);
        }

        public List<string> GetExpressionParameters(string expression, bool b)
        {
            return Parser.GetExpressionParameters(expression, this, b);
        }

        public SimpleDerivativeParser GetDeriveParser(ExpressionContext context = null)
        {
            return Parser.GetDeriveParser(context ?? this, true);
        }
    }
}
