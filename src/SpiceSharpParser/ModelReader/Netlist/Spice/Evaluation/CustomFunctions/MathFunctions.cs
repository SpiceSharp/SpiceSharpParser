using System;
using System.Numerics;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions
{
    public class MathFunctions
    {
        /// <summary>
        /// Create a pow() custom function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pow spice function.
        /// </returns>
        public static CustomFunction CreatePow(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "pow";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.Logic = (args, simulation) =>
            {
                double x = (double)args[1];
                double y = (double)args[0];

                switch (mode)
                {
                    case SpiceEvaluatorMode.LtSpice:
                        if (x < 0)
                        {
                            var realResult = Complex.Pow(new Complex(x, 0), new Complex(y, 0)).Real;

                            // TODO: remove a hack below, write a good implementation of Complex numbers for C# ...
                            if (Math.Abs(realResult) < 1e-15)
                            {
                                return 0;
                            }
                        }
                        return Math.Pow(x, y);

                    case SpiceEvaluatorMode.SmartSpice:
                        return Math.Pow(Math.Abs(x), (int)y);

                    case SpiceEvaluatorMode.HSpice:
                        return Math.Pow(x, (int)y);

                    default:
                        return Math.Pow(x, y);
                }
            };

            return function;
        }

        /// <summary>
        /// Create a sqrt custom function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pow spice function.
        /// </returns>
        public static CustomFunction CreateSqrt(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "sqrt";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (args, simulation) =>
            {
                double x = (double)args[0];

                switch (mode)
                {
                    case SpiceEvaluatorMode.LtSpice:
                        if (x < 0)
                        {
                            var realResult = Complex.Pow(new Complex(x, 0), new Complex(0.5, 0)).Real;

                            // TODO: remove a hack below, write a good implementation of Complex numbers for C# ...
                            if (Math.Abs(realResult) < 1e-15)
                            {
                                return 0;
                            }
                        }
                        return Math.Sqrt(x);

                    case SpiceEvaluatorMode.SmartSpice:
                        return Math.Sqrt(Math.Abs(x));

                    case SpiceEvaluatorMode.HSpice:
                        if (x < 0)
                        {
                            return -Math.Sqrt(Math.Abs(x));
                        }
                        else
                        {
                            return Math.Sqrt(x);
                        }

                    default:
                        return Math.Sqrt(x);
                }
            };

            return function;
        }

        /// <summary>
        /// Create a ** custom function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of ** spice function.
        /// </returns>
        public static CustomFunction CreatePowInfix(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "**";
            function.Infix = true;
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.Logic = (args, simulation) =>
            {
                double x = (double)args[1];
                double y = (double)args[0];

                switch (mode)
                {
                    case SpiceEvaluatorMode.LtSpice:
                        if (x < 0)
                        {
                            var realResult = Complex.Pow(new Complex(x, 0), new Complex(y, 0)).Real;

                            // TODO: remove a hack below, write a good implementation of Complex numbers for C# ...
                            if (Math.Abs(realResult) < 1e-15)
                            {
                                return 0;
                            }
                        }

                        return Math.Pow(x, y);

                    case SpiceEvaluatorMode.SmartSpice:
                        throw new Exception("** is unknown function");

                    case SpiceEvaluatorMode.HSpice:
                        if (x < 0)
                        {
                            return Math.Pow(x, (int)y);
                        }
                        else if (x == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            return Math.Pow(x, y);
                        }

                    default:
                        return Math.Pow(x, y);
                }
            };

            return function;
        }

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
