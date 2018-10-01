using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Evaluation.Expressions;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionRegistry"/> class.
        /// </summary>
        /// <param name="isParameterNameCaseSensitive">Is parameter name case-sensitive.</param>
        /// <param name="isExpressionNameCaseSensitive">Is expression name case-sensitivie.</param>
        public ExpressionRegistry(bool isParameterNameCaseSensitive, bool isExpressionNameCaseSensitive)
        {
            IsExpressionNameCaseSensitive = isExpressionNameCaseSensitive;
            IsParamterNameCaseSensitive = isParameterNameCaseSensitive;
            NamedExpressions = new Dictionary<string, NamedExpression>(StringComparerFactory.Create(isExpressionNameCaseSensitive));
            ParametersExpressionsDependencies = new Dictionary<string, List<Expression>>(StringComparerFactory.Create(isParameterNameCaseSensitive));
            ParametersDependencies = new Dictionary<string, HashSet<string>>(StringComparerFactory.Create(isParameterNameCaseSensitive));
            UnnamedExpressions = new List<Expression>();
        }

        public bool IsParamterNameCaseSensitive { get; }

        public bool IsExpressionNameCaseSensitive { get; }

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
        /// Gets expression names.
        /// </summary>
        /// <returns>
        /// Enumerable of expression names.
        /// </returns>
        public HashSet<string> GetExpressionNames()
        {
            return new HashSet<string>(NamedExpressions.Keys, StringComparerFactory.Create(IsExpressionNameCaseSensitive));
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

        /// <summary>
        /// Returns whether expression with given name exits.
        /// </summary>
        /// <param name="expressionName">Expression name.</param>
        /// <returns>
        /// True if expression exists.
        /// </returns>
        public bool HasExpression(string expressionName)
        {
            if (expressionName == null)
            {
                throw new ArgumentNullException(nameof(expressionName));
            }

            return NamedExpressions.ContainsKey(expressionName);
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
        public void UpdateParameterDependencies(string parameterName, ICollection<string> dependentParameters)
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
                if (ParametersDependencies.ContainsKey(parameter) == false)
                {
                    ParametersDependencies[parameter] = new HashSet<string>();
                }

                if (ParametersDependencies[parameter].Contains(parameterName) == false)
                {
                    ParametersDependencies[parameter].Add(parameterName);
                }
            }
        }

        /// <summary>
        /// Refreshes the expressions in the registry that depends on the given parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterEval">Evaluation function.</param>
        public void RefreshDependentParameters(string parameterName, Action<string> parameterEval)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (ParametersDependencies.ContainsKey(parameterName))
            {
                foreach (var parameter in ParametersDependencies[parameterName])
                {
                    parameterEval?.Invoke(parameter);
                    RefreshDependentParameters(parameter, parameterEval);
                }
            }

            if (ParametersExpressionsDependencies.ContainsKey(parameterName))
            {
                foreach (var expression in ParametersExpressionsDependencies[parameterName])
                {
                    expression.Invalidate();
                    expression.Evaluate();
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
            var result = new ExpressionRegistry(IsParamterNameCaseSensitive, IsExpressionNameCaseSensitive);

            foreach (var dep in ParametersDependencies)
            {
                result.ParametersDependencies[dep.Key] = new HashSet<string>(dep.Value);
            }

            List<Expression> addedExpresions = new List<Expression>();
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
                        addedExpresions.Add(expr);
                    }

                    if (expr is NamedExpression ne)
                    {
                        addedExpresions.Add(expr);
                        result.NamedExpressions[ne.Name] = ne;
                    }
                }
            }

            foreach (var expression in NamedExpressions)
            {
                if (!addedExpresions.Contains(expression.Value))
                {
                    result.NamedExpressions.Add(expression.Key,  (NamedExpression)expression.Value.Clone());
                }
            }

            foreach (var expression in UnnamedExpressions)
            {
                if (!addedExpresions.Contains(expression))
                {
                    result.UnnamedExpressions.Add(expression.Clone());
                }
            }

            return result;
        }

        /// <summary>
        /// Invalidates the registry.
        /// </summary>
        public void Invalidate(IEvaluator newEvaluator)
        {
            foreach (var exprDep in ParametersExpressionsDependencies)
            {
                foreach (var expr in exprDep.Value)
                {
                    expr.Evaluator = newEvaluator;
                    expr.Invalidate();
                }
            }

            foreach (var expression in NamedExpressions.Values)
            {
                expression.Evaluator = newEvaluator;
                expression.Invalidate();
            }

            foreach (var expression in UnnamedExpressions)
            {
                expression.Evaluator = newEvaluator;
                expression.Invalidate();
            }
        }
    }
}
