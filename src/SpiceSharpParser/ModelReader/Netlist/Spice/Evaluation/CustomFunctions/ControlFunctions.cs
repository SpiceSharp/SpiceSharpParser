using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions
{
    public class ControlFunctions
    {
        /// <summary>
        /// Create a def() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of def custom function.
        /// </returns>
        public static CustomFunction CreateDef(SpiceEvaluator evaluator)
        {
            CustomFunction function = new CustomFunction();
            function.Name = "def";
            function.VirtualParameters = true;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, simulation) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("def() function expects one argument");
                }

                return evaluator.HasParameter(args[0].ToString()) ? 1 : 0;
            };

            return function;
        }
    }
}
