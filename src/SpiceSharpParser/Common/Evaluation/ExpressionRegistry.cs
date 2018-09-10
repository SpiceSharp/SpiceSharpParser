using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionRegistry"/> class.
        /// </summary>
        public ExpressionRegistry()
        {
            NamedExpressions = new Dictionary<string, NamedExpression>();
            ExpressionsDependencies = new Dictionary<string, List<EvaluatorExpression>>();
            ParametersDependencies = new Dictionary<string, HashSet<string>>();
            UnnamedExpressions = new List<EvaluatorExpression>();
        }

        /// <summary>
        /// Gets the dictionary of named expressions.
        /// </summary>
        protected Dictionary<string, NamedExpression> NamedExpressions { get; }

        /// <summary>
        /// Gets the collection of unnamed expressions.
        /// </summary>
        protected ICollection<EvaluatorExpression> UnnamedExpressions { get; }

        /// <summary>
        /// Gets the dictionary of dependent parameters on parameter.
        /// </summary>
        protected Dictionary<string, HashSet<string>> ParametersDependencies { get; }

        /// <summary>
        /// Gets the dictionary of dependent expressions on parameter.
        /// </summary>
        protected Dictionary<string, List<EvaluatorExpression>> ExpressionsDependencies { get; }

        /// <summary>
        /// Gets expressions that depend on given parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// An enumerable of expressions.
        /// </returns>
        public IEnumerable<EvaluatorExpression> GetDependentExpressions(string parameterName)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (ExpressionsDependencies.ContainsKey(parameterName))
            {
                return ExpressionsDependencies[parameterName];
            }
            else
            {
                return new List<EvaluatorExpression>();
            }
        }

        /// <summary>
        /// Gets expression names.
        /// </summary>
        /// <returns>
        /// Enumerable of expressio names.
        /// </returns>
        public IEnumerable<string> GetExpressionNames()
        {
            return NamedExpressions.Keys;
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
        public void Add(EvaluatorExpression expression, ICollection<string> parameters)
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
                if (!ExpressionsDependencies.ContainsKey(parameter))
                {
                    ExpressionsDependencies[parameter] = new List<EvaluatorExpression>();
                }

                ExpressionsDependencies[parameter].Add(expression);
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
                if (!ExpressionsDependencies.ContainsKey(parameter))
                {
                    ExpressionsDependencies[parameter] = new List<EvaluatorExpression>();
                }

                ExpressionsDependencies[parameter].RemoveAll(r => r is NamedExpression n && n.Name == namedExpression.Name);

                ExpressionsDependencies[parameter].Add(namedExpression);
            }

            if (this.NamedExpressions.ContainsKey(namedExpression.Name))
            {
                this.NamedExpressions.Remove(namedExpression.Name);
            }

            this.NamedExpressions.Add(namedExpression.Name, namedExpression);
        }

        /// <summary>
        /// Updates parameter dependencies.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="dependentParameters">Dependent paramaters.</param>
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
        /// <param name="context">Context.</param>
        /// <param name="parameterEval">Evaluation function.</param>
        public void RefreshDependentParameters(string parameterName, object context, Action<string> parameterEval)
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
                    RefreshDependentParameters(parameter, context, parameterEval);
                }
            }

            if (ExpressionsDependencies.ContainsKey(parameterName))
            {
                foreach (var expression in ExpressionsDependencies[parameterName])
                {
                    expression.Invalidate();
                    expression.Evaluate(context);
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
            var result = new ExpressionRegistry();

            foreach (var dep in ParametersDependencies)
            {
                result.ParametersDependencies[dep.Key] = new HashSet<string>(dep.Value);
            }

            List<EvaluatorExpression> addedExpresions = new List<EvaluatorExpression>();
            foreach (var exprDep in ExpressionsDependencies)
            {
                foreach (var expr in exprDep.Value)
                {
                    if (!result.ExpressionsDependencies.ContainsKey(exprDep.Key))
                    {
                        result.ExpressionsDependencies[exprDep.Key] = new List<EvaluatorExpression>();
                    }

                    var clone = expr.Clone();
                    result.ExpressionsDependencies[exprDep.Key].Add(clone);

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
        /// Updates the evaluator of the registry.
        /// </summary>
        /// <param name="newEvaluator">New evaluator for registry.</param>
        public void UpdateEvaluator(IEvaluator newEvaluator)
        {
            foreach (var exprDep in ExpressionsDependencies)
            {
                foreach (var expr in exprDep.Value)
                {
                    expr.Evaluator = newEvaluator;
                }
            }

            foreach (var expression in NamedExpressions.Values)
            {
                expression.Evaluator = newEvaluator;
            }

            foreach (var expression in UnnamedExpressions)
            {
                expression.Evaluator = newEvaluator;
            }
        }

        /// <summary>
        /// Invalidates the registry.
        /// </summary>
        public void Invalidate()
        {
            foreach (var exprDep in ExpressionsDependencies)
            {
                foreach (var expr in exprDep.Value)
                {
                    expr.Invalidate();
                }
            }

            foreach (var expression in NamedExpressions.Values)
            {
                expression.Invalidate();
            }

            foreach (var expression in UnnamedExpressions)
            {
                expression.Invalidate();
            }
        }
    }
}
