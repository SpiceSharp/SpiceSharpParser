using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Evaluation
{
    public class ExpressionRegistry
    {
        List<DoubleExpression> expressions = new List<DoubleExpression>();

        /// <summary>
        /// Gets expressions that depend on given parameter
        /// </summary>
        /// <param name="parameterName">A parameter name</param>
        /// <returns>
        /// An enumerable of expressions
        /// </returns>
        public IEnumerable<DoubleExpression> GetDependedExpressions(string parameterName)
        {
            //TODO: implement someday something better

            return expressions;
        }

        /// <summary>
        /// Adds an expression to registry
        /// </summary>
        /// <param name="expression">An expression to add</param>
        public void Add(DoubleExpression expression)
        {
            expressions.Add(expression);
        }
    }
}
