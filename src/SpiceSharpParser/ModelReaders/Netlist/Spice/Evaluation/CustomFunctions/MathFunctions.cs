using System;
using System.Numerics;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions
{
    public class MathFunctions
    {
        /// <summary>
        /// Create a pow() custom function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pow custom function.
        /// </returns>
        public static CustomFunction CreatePow(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "pow";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.Logic = (args, context, evaluator) =>
            {
                double x = (double)args[0];
                double y = (double)args[1];

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
        /// Create a pwr() custom function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pwr custom function.
        /// </returns>
        public static CustomFunction CreatePwr(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "pwr";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.Logic = (args, context, evaluator) =>
            {
                double x = (double)args[0];
                double y = (double)args[1];

                switch (mode)
                {
                    case SpiceEvaluatorMode.LtSpice:
                        return Math.Pow(Math.Abs(x), y);

                    case SpiceEvaluatorMode.HSpice:
                    case SpiceEvaluatorMode.SmartSpice:
                        return Math.Sign(x) * Math.Pow(Math.Abs(x), y);

                    default:
                        return Math.Pow(x, y); //TODO: define logic for default
                }
            };

            return function;
        }

        /// <summary>
        /// Create a pwrs() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of pwrs custom function.
        /// </returns>
        public static CustomFunction CreatePwrs()
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "pwrs";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.Logic = (args, context, evaluator) =>
            {
                double x = (double)args[0];
                double y = (double)args[1];

                return Math.Sign(x) * Math.Pow(Math.Abs(x), y);
            };

            return function;
        }

        /// <summary>
        /// Create a sqrt custom function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pow custom function.
        /// </returns>
        public static CustomFunction CreateSqrt(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "sqrt";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.Logic = (args, context, evaluator) =>
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
        /// A new instance of ** custom function.
        /// </returns>
        public static CustomFunction CreatePowInfix(SpiceEvaluatorMode mode)
        {
            Random randomGenerator = new Random(Environment.TickCount);

            CustomFunction function = new CustomFunction();
            function.Name = "**";
            function.Infix = true;
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.Logic = (args, context, evaluator) =>
            {
                double x = (double)args[0];
                double y = (double)args[1];

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
        /// A new instance of min custom function.
        /// </returns>
        public static CustomFunction CreateMin()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "min";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
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
        /// A new instance of min custom function.
        /// </returns>
        public static CustomFunction CreateMax()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "max";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
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

        /// <summary>
        /// Create a limit(x, xmin, xmax) custom function.
        /// </summary>
        /// <returns>
        /// A new instance of limit custom function.
        /// </returns>
        public static CustomFunction CreateLimit()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "limit";
            function.VirtualParameters = false;
            function.ArgumentsCount = 3;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 3)
                {
                    throw new ArgumentException("limit() function expects 3 arguments");
                }

                double x = (double)args[0];
                double xMin = (double)args[1];
                double xMax = (double)args[2];

                if (x < xMin)
                {
                    return xMin;
                }

                if (x > xMax)
                {
                    return xMax;
                }

                return x;
            };

            return function;
        }

        /// <summary>
        /// Create a ln() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of ln custom function.
        /// </returns>
        public static CustomFunction CreateLn()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "ln";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("ln() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Log(x);
            };

            return function;
        }

        /// <summary>
        /// Create a log() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of log custom function.
        /// </returns>
        public static CustomFunction CreateLog(SpiceEvaluatorMode mode)
        {
            CustomFunction function = new CustomFunction();
            function.Name = "log";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("log() function expects one argument");
                }

                double x = (double)args[0];

                if (mode == SpiceEvaluatorMode.HSpice)
                {
                    return Math.Sign(x) * Math.Log(Math.Abs(x));
                }

                return Math.Log(x);
            };

            return function;
        }

        /// <summary>
        /// Create a log10() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of log10 custom function.
        /// </returns>
        public static CustomFunction CreateLog10(SpiceEvaluatorMode mode)
        {
            CustomFunction function = new CustomFunction();
            function.Name = "log10";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("log10() function expects one argument");
                }

                double x = (double)args[0];

                if (mode == SpiceEvaluatorMode.HSpice)
                {
                    return Math.Sign(x) * Math.Log10(Math.Abs(x));
                }

                return Math.Log10(x);
            };

            return function;
        }

        /// <summary>
        /// Create a cbrt() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of cbrt custom function.
        /// </returns>
        public static CustomFunction CreateCbrt()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "cbrt";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("cbrt() function expects one argument");
                }

                double x = (double)args[0];

                return Math.Pow(x, 1.0 / 3.0);
            };

            return function;
        }

        /// <summary>
        /// Create a buf() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of buf custom function.
        /// </returns>
        public static CustomFunction CreateBuf()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "buf";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("cbrt() function expects one argument");
                }

                double x = (double)args[0];

                return x > 0.5 ? 1 : 0;
            };

            return function;
        }

        /// <summary>
        /// Create a ceil() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of ceil custom function.
        /// </returns>
        public static CustomFunction CreateCeil()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "ceil";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("ceil() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Ceiling(x);
            };

            return function;
        }

        /// <summary>
        /// Create a abs() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of abs custom function.
        /// </returns>
        public static CustomFunction CreateAbs()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "abs";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("abs() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Abs(x);
            };

            return function;
        }

        /// <summary>
        /// Create a floor() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of floor custom function.
        /// </returns>
        public static CustomFunction CreateFloor()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "floor";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("floor() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Floor(x);
            };

            return function;
        }

        /// <summary>
        /// Create a hypot() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of hypot custom function.
        /// </returns>
        public static CustomFunction CreateHypot()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "hypot";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 2)
                {
                    throw new ArgumentException("hypot() function expects three arguments");
                }

                double x = (double)args[0];
                double y = (double)args[1];

                return Math.Sqrt((x * x) + (y * y));
            };

            return function;
        }

        /// <summary>
        /// Create a int() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of int custom function.
        /// </returns>
        public static CustomFunction CreateInt()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "int";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("int() function expects one argument");
                }

                double x = (double)args[0];
                return (int)x;
            };

            return function;
        }

        /// <summary>
        /// Create a inv() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of int custom function.
        /// </returns>
        public static CustomFunction CreateInv()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "inv";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("inv() function expects one argument");
                }

                double x = (double)args[0];

                return x > 0.5 ? 0 : 1;
            };

            return function;
        }

        /// <summary>
        /// Create a exp() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of exp custom function.
        /// </returns>
        public static CustomFunction CreateExp()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "exp";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("exp() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Exp(x);
            };

            return function;
        }

        /// <summary>
        /// Create a db() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of db custom function.
        /// </returns>
        public static CustomFunction CreateDb(SpiceEvaluatorMode mode)
        {
            CustomFunction function = new CustomFunction();
            function.Name = "db";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("db() function expects one argument");
                }

                double x = (double)args[0];

                if (mode == SpiceEvaluatorMode.SmartSpice)
                {
                    return 20.0 * Math.Log10(Math.Abs(x));
                }

                return Math.Sign(x) * 20.0 * Math.Log10(Math.Abs(x));
            };

            return function;
        }

        /// <summary>
        /// Create a round() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of round custom function.
        /// </returns>
        public static CustomFunction CreateRound()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "round";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("round() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Round(x);
            };

            return function;
        }

        /// <summary>
        /// Create a u() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of u custom function.
        /// </returns>
        public static CustomFunction CreateU()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "u";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("u() function expects one argument");
                }

                double x = (double)args[0];
                return x > 0 ? 1 : 0;
            };

            return function;
        }

        /// <summary>
        /// Create a uramp() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of uramp custom function.
        /// </returns>
        public static CustomFunction CreateURamp()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "uramp";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("uramp() function expects one argument");
                }

                double x = (double)args[0];
                return x > 0 ? x : 0;
            };

            return function;
        }

        /// <summary>
        /// Create a sgn() custom function.
        /// </summary>
        /// <returns>
        /// A new instance of sgn custom function.
        /// </returns>
        public static CustomFunction CreateSgn()
        {
            CustomFunction function = new CustomFunction();
            function.Name = "sgn";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.Logic = (args, context, evaluator) =>
            {
                if (args.Length != 1)
                {
                    throw new ArgumentException("sgn() function expects one argument");
                }

                double x = (double)args[0];
                return Math.Sign(x);
            };

            return function;
        }
    }
}
