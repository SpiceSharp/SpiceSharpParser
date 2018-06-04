using System;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions
{
    public class ControlFunctions
    {
        /// <summary>
        /// Create a def() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of def custom function.
        /// </returns>
        public static CustomFunction CreateDef()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "def";
            function.VirtualParameters = true;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("def() function expects one argument");
                }

                return evaluator.HasParameter(args[0].ToString()) ? 1 : 0;
            };

            return function;
        }

        /// <summary>
        /// Create a lazy() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of lazy custom function.
        /// </returns>
        public static CustomFunction CreateLazy()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "lazy";
            function.VirtualParameters = true;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("lazy() function expects one argument");
                }

                return evaluator.CreateChildEvaluator().EvaluateDouble(args[0].ToString(), context);
            };

            return function;
        }

        /// <summary>
        /// Create a if() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of if custom function.
        /// </returns>
        public static CustomFunction CreateIf()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "if";
            function.VirtualParameters = false;
            function.ArgumentsCount = 3;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 3)
                {
                    throw new ArgumentException("if() function expects three arguments");
                }

                double x = (double)args[2];
                double y = (double)args[1];
                double z = (double)args[0];

                if (x > 0.5)
                {
                    return y;
                }
                else
                {
                    return z;
                }
            };

            return function;
        }
    }
}
