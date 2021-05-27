using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class FunctionFactory : IFunctionFactory
    {
        /// <summary>
        /// Creates a new function.
        /// </summary>
        /// <param name="name">Name of a function.</param>
        /// <param name="arguments">Arguments of a function.</param>
        /// <param name="functionBodyExpression">Body expression of a function.</param>
        public IFunction<double, double> Create(
            string name,
            List<string> arguments,
            string functionBodyExpression)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (functionBodyExpression == null)
            {
                throw new ArgumentNullException(nameof(functionBodyExpression));
            }

            return new ExpressionFunction(name, arguments, functionBodyExpression);
        }
    }
}