using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public class ExpressionRegistry
    {
        private readonly Dictionary<string, List<ActionExpression>> expressionsByParameterName = new Dictionary<string, List<ActionExpression>>();
        private readonly Dictionary<string, ActionExpression> expressionsByName = new Dictionary<string, ActionExpression>();

        /// <summary>
        /// Gets expressions that depend on given parameter.
        /// </summary>
        /// <param name="parameterName">A parameter name.</param>
        /// <returns>
        /// An enumerable of expressions.
        /// </returns>
        public IEnumerable<ActionExpression> GetDependentExpressions(string parameterName)
        {
            if (expressionsByParameterName.ContainsKey(parameterName))
            {
                return expressionsByParameterName[parameterName];
            }
            else
            {
                return new List<ActionExpression>();
            }
        }

        /// <summary>
        /// Adds an expression to registry.
        /// </summary>
        /// <param name="expression">An expression to add.</param>
        /// <param name="expressionParameters">A list of expression parameters.</param>
        public void Add(ActionExpression expression, IEnumerable<string> expressionParameters)
        {
            foreach (var parameter in expressionParameters)
            {
                if (expressionsByParameterName.ContainsKey(parameter))
                {
                    expressionsByParameterName[parameter].Add(expression);
                }
                else
                {
                    expressionsByParameterName[parameter] = new List<ActionExpression>() { expression };
                }
            }
        }

        /// <summary>
        /// Adds named expression to registry.
        /// </summary>
        /// <param name="expressionName">An expression name to add.</param>
        /// <param name="expression">An expression to add.</param>
        /// <param name="expressionParameters">A list of expression parameters</param>
        public void Add(string expressionName, ActionExpression expression, IEnumerable<string> expressionParameters)
        {
            expressionsByName[expressionName] = expression;
            Add(expression, expressionParameters);
        }

        /// <summary>
        /// Gets expression names.
        /// </summary>
        /// <returns>
        /// Names of expressions.
        /// </returns>
        public IEnumerable<string> GetExpressionNames()
        {
            return expressionsByName.Keys;
        }

        /// <summary>
        /// Gets the expression with given name.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <returns>
        /// An expression with given name.
        /// </returns>
        public string GetExpression(string expressionName)
        {
            return expressionsByName[expressionName].Expression;
        }
    }
}
