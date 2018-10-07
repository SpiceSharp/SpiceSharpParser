using System;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class ControlFunctions
    {
        /// <summary>
        /// Get a def() function.
        /// </summary>
        /// <returns>
        /// A new instance of def function.
        /// </returns>
        public static Function CreateDef()
        {
            Function function = new Function();
            function.Name = "def";
            function.VirtualParameters = true;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (image, args, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("def() function expects one argument");
                }

                return evaluator.Parameters.ContainsKey(args[0].ToString()) ? 1 : 0;
            };

            return function;
        }

        /// <summary>
        /// Get a lazy() function.
        /// </summary>
        /// <returns>
        /// A new instance of lazy  function.
        /// </returns>
        public static Function CreateLazy()
        {
            Function function = new Function();
            function.Name = "lazy";
            function.VirtualParameters = true;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (image, args, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("lazy() function expects one argument");
                }

                return evaluator.CreateChildEvaluator(evaluator.Name + "_lazy", evaluator.Context).EvaluateDouble(args[0].ToString());
            };

            return function;
        }

        /// <summary>
        /// Get a if() function.
        /// </summary>
        /// <returns>
        /// A new instance of if function.
        /// </returns>
        public static Function CreateIf()
        {
            Function function = new Function();
            function.Name = "if";
            function.VirtualParameters = false;
            function.ArgumentsCount = 3;
            function.ReturnType = typeof(double);

            function.Logic = (image, args, evaluator) =>
            {
                if (args.Length != 3)
                {
                    throw new ArgumentException("if() function expects three arguments");
                }

                double x = (double)args[0];
                double y = (double)args[1];
                double z = (double)args[2];

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
