using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions
{
    public class TableFunction
    {
        /// <summary>
        /// Create a table() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of random spice function.
        /// </returns>
        public static CustomFunction Create()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "table";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;

            function.Logic = (args, context, evaluator) =>
            {
                var functionEvaluator = evaluator.CreateChildEvaluator();
                var parameter = args[0];
                var parameterValue = functionEvaluator.EvaluateDouble(parameter.ToString());

                for (var i = 1; i < args.Length - 1; i += 2)
                {
                    var pointX = functionEvaluator.EvaluateDouble(args[i].ToString());
                    var pointY = functionEvaluator.EvaluateDouble(args[i + 1].ToString());

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
