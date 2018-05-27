using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions
{
    public class MathFunctions
    {
        /// <summary>
        /// Create a min() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of min spice function.
        /// </returns>
        public static CustomFunction CreateMin()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "min";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;
            function.ReturnType = typeof(double);

            function.Logic = (args, simulation) =>
            {
                if (args.Length == 0)
                {
                    throw new ArgumentException("Min() function expects arguments");
                }

                double min = (double)args[0];

                for (var i = 1; i < args.Length; i++)
                {
                    if ((double)args[i] < min)
                    {
                        min = (double)args[i];
                    }
                }

                return min;
            };

            return function;
        }

        /// <summary>
        /// Create a max() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of min spice function.
        /// </returns>
        public static CustomFunction CreateMax()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "max";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;
            function.ReturnType = typeof(double);

            function.Logic = (args, simulation) =>
            {
                if (args.Length == 0)
                {
                    throw new ArgumentException("Max() function expects arguments");
                }

                double max = (double)args[0];

                for (var i = 1; i < args.Length; i++)
                {
                    if ((double)args[i] > max)
                    {
                        max = (double)args[i];
                    }
                }

                return max;
            };

            return function;
        }
    }
}
