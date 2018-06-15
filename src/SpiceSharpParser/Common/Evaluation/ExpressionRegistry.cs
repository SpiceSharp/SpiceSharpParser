using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionRegistry
    {
        public ExpressionRegistry()
        {
            NamedExpressions = new Dictionary<string, NamedExpression>();
            ExpressionsDependencies = new Dictionary<string, List<EvaluatorExpression>>();
            ParametersDependencies = new Dictionary<string, List<string>>();
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
        protected Dictionary<string, List<string>> ParametersDependencies { get; }

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
            if (ExpressionsDependencies.ContainsKey(parameterName))
            {
                return ExpressionsDependencies[parameterName];
            }
            else
            {
                return new List<EvaluatorExpression>();
            }
        }

        public IEnumerable<string> GetExpressionNames()
        {
            return NamedExpressions.Keys;
        }

        public EvaluatorExpression GetExpression(string expressionName)
        {
            return NamedExpressions[expressionName];
        }

        public bool HasExpression(string expressionName)
        {
            return NamedExpressions.ContainsKey(expressionName);
        }

        public void Add(EvaluatorExpression evaluatorExpression, ICollection<string> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!ExpressionsDependencies.ContainsKey(parameter))
                {
                    ExpressionsDependencies[parameter] = new List<EvaluatorExpression>();
                }

                ExpressionsDependencies[parameter].Add(evaluatorExpression);
            }

            this.UnnamedExpressions.Add(evaluatorExpression);
        }

        public void Add(NamedExpression namedExpression, ICollection<string> parameters)
        {
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

        public void UpdateParameterDependencies(string parameterName, ICollection<string> foundParameters)
        {
            foreach (var parameter in foundParameters)
            {
                if (ParametersDependencies.ContainsKey(parameter) == false)
                {
                    ParametersDependencies[parameter] = new List<string>();
                }

                if (ParametersDependencies[parameter].Contains(parameterName) == false)
                {
                    ParametersDependencies[parameter].Add(parameterName);
                }
            }
        }

        public void RefreshDependentParameters(string parameterName, object context, Action<string> parameterEval)
        {
            if (ParametersDependencies.ContainsKey(parameterName))
            {
                foreach (var parameter in ParametersDependencies[parameterName])
                {
                    parameterEval(parameter);
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
    }
}
