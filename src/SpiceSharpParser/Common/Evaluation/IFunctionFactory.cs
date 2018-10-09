using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    /// <summary>
    /// Interface for all function factories.
    /// </summary>
    public interface IFunctionFactory
    {
        /// <summary>
        /// Creates a new function.
        /// </summary>
        /// <param name="name">Name of a function.</param>
        /// <param name="arguments">Arguments of a function.</param>
        /// <param name="functionBodyExpression">Body expression of a function.</param>
        Function Create(
            string name,
            List<string> arguments,
            string functionBodyExpression);
    }
}
