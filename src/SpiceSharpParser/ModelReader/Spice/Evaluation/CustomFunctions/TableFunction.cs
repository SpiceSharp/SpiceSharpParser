using System;
using System.Collections.Generic;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Spice.Processors;

namespace SpiceSharpParser.ModelReader.Spice.Evaluation.CustomFunctions
{
    public class TableFunction
    {
        /// <summary>
        /// Creates table custom function.
        /// </summary>
        public static SpiceFunction Create(IEvaluator evaluator)
        {
            return CreateTable(evaluator);
        }

        /// <summary>
        /// Create a table() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of random spice function.
        /// </returns>
        public static SpiceFunction CreateTable(IEvaluator evaluator)
        {
            SpiceFunction function = new SpiceFunction();
            function.Name = "table";
            function.VirtualParameters = true;

            function.Logic = (args, simulation) =>
            {
                var functionEvaluator = new Evaluator(evaluator);
                var parameter = args[args.Length - 1];
                var parameterValue = functionEvaluator.EvaluateDouble(parameter.ToString());

                for (var i = args.Length - 2; i >= 0; i -= 2)
                {
                    var pointX = functionEvaluator.EvaluateDouble(args[i].ToString());
                    var pointY = functionEvaluator.EvaluateDouble(args[i - 1].ToString());

                    if (pointX == parameterValue)
                    {
                        return pointY;
                    }
                }

                return 0;
            };

            return function;
        }
    }
}
