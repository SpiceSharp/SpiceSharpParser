using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class FunctionFactory : IFunctionFactory
    {
        /// <summary>
        /// Creates a new function.
        /// </summary>
        /// <param name="name">Name of a function.</param>
        /// <param name="arguments">Arguments of a function.</param>
        /// <param name="functionBodyExpression">Body expression of a function.</param>
        public Function Create(
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

            Function userFunction = new Function();
            userFunction.Name = name;
            userFunction.VirtualParameters = false;
            userFunction.ArgumentsCount = arguments.Count;

            userFunction.DoubleArgsLogic = (image, args, evaluator) =>
            {
                var childEvaluator = evaluator.CreateChildEvaluator(evaluator.Name + "_" + name, evaluator.Context);
                childEvaluator.Registry.RemoveExpression(functionBodyExpression);

                for (var i = 0; i < arguments.Count; i++)
                {
                    childEvaluator.SetParameter(arguments[i], args[i]);
                }

                var expression = new Expression(functionBodyExpression, childEvaluator);
                var result = expression.Evaluate();

                return result;
            };

            return userFunction;
        }
    }
}
