using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Evaluation
{
    public class ExpressionRegistry
    {
        readonly Dictionary<string, List<DoubleExpression>> expressions = new Dictionary<string, List<DoubleExpression>>();

        /// <summary>
        /// Gets expressions that depend on given parameter
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        /// <returns>
        /// An enumerable of expressions
        /// </returns>
        public IEnumerable<DoubleExpression> GetDependentExpressions(string parameterName)
        {
            if (expressions.ContainsKey(parameterName))
            {
                return expressions[parameterName];
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
        public void Add(DoubleExpression expression, List<string> expressionParameters)
        {
            foreach (var parameter in expressionParameters)
            {
                if (expressions.ContainsKey(parameter))
                {
                    expressions[parameter].Add(expression);
                }
                else
                {
                    expressions[parameter] = new List<DoubleExpression>() { expression };
                }
            }
        }
    }
}
