using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReader.Spice.Evaluation
{
    public class ExpressionRegistry
    {
        private readonly Dictionary<string, List<DoubleExpression>> expressionsByParameterName = new Dictionary<string, List<DoubleExpression>>();
        private readonly Dictionary<string, DoubleExpression> expressionsByName = new Dictionary<string, DoubleExpression>();

        /// <summary>
        /// Gets expressions that depend on given parameter
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        /// <returns>
        /// An enumerable of expressions
        /// </returns>
        public IEnumerable<DoubleExpression> GetDependentExpressions(string parameterName)
        {
            if (expressionsByParameterName.ContainsKey(parameterName))
            {
                return expressionsByParameterName[parameterName];
            }
            else
            {
                return new List<DoubleExpression>();
            }
        }

        /// <summary>
        /// Adds an expression to registry
        /// </summary>
        /// <param name="expression">An expression to add</param>
        /// <param name="expressionParameters">A list of expression parameters</param>
        public void Add(DoubleExpression expression, IEnumerable<string> expressionParameters)
        {
            foreach (var parameter in expressionParameters)
            {
                if (expressionsByParameterName.ContainsKey(parameter))
                {
                    expressionsByParameterName[parameter].Add(expression);
                }
                else
                {
                    expressionsByParameterName[parameter] = new List<DoubleExpression>() { expression };
                }
            }
        }

        /// <summary>
        /// Adds named expression to registry
        /// </summary>
        /// <param name="expressionName">An expression name to add</param>
        /// <param name="expression">An expression to add</param>
        /// <param name="expressionParameters">A list of expression parameters</param>
        public void Add(string expressionName, DoubleExpression expression, IEnumerable<string> expressionParameters)
        {
            expressionsByName[expressionName] = expression;
            Add(expression, expressionParameters);
        }

        /// <summary>
        /// Gets expression names
        /// </summary>
        /// <returns>
        /// Names of expressions
        /// </returns>
        public IEnumerable<string> GetExpressionNames()
        {
            return expressionsByName.Keys;
        }

        /// <summary>
        /// Gets the expression
        /// </summary>
        /// <param name="expressionName">Name of expression</param>
        /// <returns>
        /// An expression
        /// </returns>
        public string GetExpression(string expressionName)
        {
            return expressionsByName[expressionName].ValueExpression;
        }
    }
}
