using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class FunctionFactory : IFunctionFactory
    {
        /// <summary>
        /// Creates a new function.
        /// </summary>
        public Function Create(
            string name,
            List<string> arguments,
            string functionBody)
        {
            Function userFunction = new Function();
            userFunction.Name = name;
            userFunction.VirtualParameters = false;
            userFunction.ArgumentsCount = arguments.Count;

            userFunction.Logic = (image, args, evaluator) =>
            {
                var childEvaluator = evaluator.CreateChildEvaluator(evaluator.Name + "_" + name, evaluator.Context);
                for (var i = 0; i < arguments.Count; i++)
                {
                    childEvaluator.SetParameter(arguments[i], (double)args[i]);
                }

                var functionBodyExpression = new Expression(functionBody, childEvaluator);

                return functionBodyExpression.Evaluate();
            };

            return userFunction;
        }
    }
}
