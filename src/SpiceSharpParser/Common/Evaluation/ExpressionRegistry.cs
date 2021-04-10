using SpiceSharpParser.Common.Evaluation.Expressions;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionRegistry"/> class.
        /// </summary>
        /// <param name="isParameterNameCaseSensitive">Is parameter name case-sensitive.</param>
        /// <param name="isExpressionNameCaseSensitive">Is expression name case-sensitive.</param>
        public ExpressionRegistry(bool isParameterNameCaseSensitive, bool isExpressionNameCaseSensitive)
        {
            IsExpressionNameCaseSensitive = isExpressionNameCaseSensitive;
            IsParameterNameCaseSensitive = isParameterNameCaseSensitive;
            NamedExpressions = new Dictionary<string, NamedExpression>(StringComparerProvider.Get(isExpressionNameCaseSensitive));
            ParametersExpressionsDependencies = new Dictionary<string, List<Expression>>(StringComparerProvider.Get(isParameterNameCaseSensitive));
            ParametersDependencies = new Dictionary<string, HashSet<string>>(StringComparerProvider.Get(isParameterNameCaseSensitive));
            UnnamedExpressions = new List<Expression>();
            Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(isParameterNameCaseSensitive));
        }

        /// <summary>
        /// Gets a value indicating whether parameter names are case sensitive.
        /// </summary>
        public bool IsParameterNameCaseSensitive { get; }

        /// <summary>
        /// Gets a value indicating whether expression names are case sensitive.
        /// </summary>
        public bool IsExpressionNameCaseSensitive { get; }

        /// <summary>
        /// Gets the dictionary of parameters.
        /// </summary>
        protected Dictionary<string, Expression> Parameters { get; }

        /// <summary>
        /// Gets the dictionary of named expressions.
        /// </summary>
        protected Dictionary<string, NamedExpression> NamedExpressions { get; }

        /// <summary>
        /// Gets the collection of unnamed expressions.
        /// </summary>
        protected ICollection<Expression> UnnamedExpressions { get; }

        /// <summary>
        /// Gets the dictionary of dependent parameters on parameter.
        /// </summary>
        protected Dictionary<string, HashSet<string>> ParametersDependencies { get; }

        /// <summary>
        /// Gets the dictionary of dependent expressions on parameter.
        /// </summary>
        protected Dictionary<string, List<Expression>> ParametersExpressionsDependencies { get; }

        /// <summary>
        /// Gets expression names.
        /// </summary>
        /// <returns>
        /// Enumerable of expression names.
        /// </returns>
        public HashSet<string> GetExpressionNames()
        {
            return new HashSet<string>(NamedExpressions.Keys, StringComparerProvider.Get(IsExpressionNameCaseSensitive));
        }

        /// <summary>
        /// Gets expressions that depend on given parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// An enumerable of expressions.
        /// </returns>
        public IEnumerable<Expression> GetDependentExpressions(string parameterName)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (ParametersExpressionsDependencies.ContainsKey(parameterName))
            {
                return ParametersExpressionsDependencies[parameterName];
            }
            else
            {
                return new List<Expression>();
            }
        }

        /// <summary>
        /// Gets the expression with given name.
        /// </summary>
        /// <param name="expressionName">Expression name.</param>
        /// <returns>
        /// A named expression.
        /// </returns>
        public NamedExpression GetExpression(string expressionName)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            return NamedExpressions[expressionName];
        }

        public void AddOrUpdate(string parameterName, Expression parameterExpression)
        {
            Parameters[parameterName] = parameterExpression;
        }

        /// <summary>
        /// Adds an expression to registry.
        /// </summary>
        /// <param name="expression">Expression to add.</param>
        /// <param name="parameters">Parameters of the expression.</param>
        public void Add(Expression expression, ICollection<string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            foreach (var parameter in parameters)
            {
                if (!ParametersExpressionsDependencies.ContainsKey(parameter))
                {
                    ParametersExpressionsDependencies[parameter] = new List<Expression>();
                }

                ParametersExpressionsDependencies[parameter].Add(expression);
            }

            UnnamedExpressions.Add(expression);
        }

        /// <summary>
        /// Adds named expression to registry.
        /// </summary>
        /// <param name="namedExpression">Named expression to add.</param>
        /// <param name="parameters">Parameters of the expression.</param>
        public void Add(NamedExpression namedExpression, ICollection<string> parameters)
        {
            if (namedExpression == null)
            {
                throw new ArgumentNullException(nameof(namedExpression));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (var parameter in parameters)
            {
                if (!ParametersExpressionsDependencies.ContainsKey(parameter))
                {
                    ParametersExpressionsDependencies[parameter] = new List<Expression>();
                }

                ParametersExpressionsDependencies[parameter].RemoveAll(r => r is NamedExpression n && n.Name == namedExpression.Name);
                ParametersExpressionsDependencies[parameter].Add(namedExpression);
            }

            if (NamedExpressions.ContainsKey(namedExpression.Name))
            {
                NamedExpressions.Remove(namedExpression.Name);
            }

            NamedExpressions.Add(namedExpression.Name, namedExpression);
        }

        /// <summary>
        /// Updates parameter dependencies.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="dependentParameters">Dependent parameters.</param>
        public void AddOrUpdateParameterDependencies(string parameterName, ICollection<string> dependentParameters)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (dependentParameters == null)
            {
                throw new ArgumentNullException(nameof(dependentParameters));
            }

            foreach (var parameter in dependentParameters)
            {
                if (!ParametersDependencies.ContainsKey(parameter))
                {
                    ParametersDependencies[parameter] = new HashSet<string>();
                }

                if (!ParametersDependencies[parameter].Contains(parameterName))
                {
                    ParametersDependencies[parameter].Add(parameterName);
                }
            }
        }

        /// <summary>
        /// Clones the registry.
        /// </summary>
        /// <returns>
        /// A clone of registry.
        /// </returns>
        public ExpressionRegistry Clone()
        {
            var result = new ExpressionRegistry(IsParameterNameCaseSensitive, IsExpressionNameCaseSensitive);

            foreach (var parameter in Parameters)
            {
                result.Parameters[parameter.Key] = parameter.Value.Clone();
            }

            foreach (var dep in ParametersDependencies)
            {
                result.ParametersDependencies[dep.Key] = new HashSet<string>(dep.Value);
            }

            List<Expression> addedExpressions = new List<Expression>();
            foreach (var exprDep in ParametersExpressionsDependencies)
            {
                foreach (var expr in exprDep.Value)
                {
                    if (!result.ParametersExpressionsDependencies.ContainsKey(exprDep.Key))
                    {
                        result.ParametersExpressionsDependencies[exprDep.Key] = new List<Expression>();
                    }

                    var clone = expr.Clone();
                    result.ParametersExpressionsDependencies[exprDep.Key].Add(clone);

                    if (UnnamedExpressions.Contains(expr))
                    {
                        result.UnnamedExpressions.Add(clone);
                        addedExpressions.Add(expr);
                    }

                    if (expr is NamedExpression ne)
                    {
                        addedExpressions.Add(expr);
                        result.NamedExpressions[ne.Name] = ne;
                    }
                }
            }

            foreach (var expression in NamedExpressions)
            {
                if (!result.NamedExpressions.ContainsKey(expression.Key))
                {
                    result.NamedExpressions.Add(expression.Key, (NamedExpression)expression.Value.Clone());
                }
            }

            foreach (var expression in UnnamedExpressions)
            {
                if (!addedExpressions.Contains(expression))
                {
                    result.UnnamedExpressions.Add(expression.Clone());
                }
            }

            return result;
        }
    }
}