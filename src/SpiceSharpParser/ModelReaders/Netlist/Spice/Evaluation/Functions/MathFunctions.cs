using System;
using System.Numerics;
using SpiceSharpParser.Common.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class MathFunctions
    {
        /// <summary>
        /// Get a pow() function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pow function.
        /// </returns>
        public static Function CreatePow(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "pow";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a pwr() function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pwr function.
        /// </returns>
        public static Function CreatePwr(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "pwr";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
                        return Math.Pow(x, y); // TODO: define logic for default
                }
            };

            return function;
        }

        /// <summary>
        /// Get a pwrs() function.
        /// </summary>
        /// <returns>
        /// A new instance of pwrs function.
        /// </returns>
        public static Function CreatePwrs()
        {
            Function function = new Function();
            function.Name = "pwrs";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.DoubleArgsLogic = (image, args, evaluator) =>
            {
                double x = (double)args[0];
                double y = (double)args[1];

                return Math.Sign(x) * Math.Pow(Math.Abs(x), y);
            };

            return function;
        }

        /// <summary>
        /// Get a sqrt function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pow function.
        /// </returns>
        public static Function CreateSqrt(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "sqrt";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a ** function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of ** function.
        /// </returns>
        public static Function CreatePowInfix(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "**";
            function.Infix = true;
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a min() function.
        /// </summary>
        /// <returns>
        /// A new instance of min function.
        /// </returns>
        public static Function CreateMin()
        {
            Function function = new Function();
            function.Name = "min";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a max() function.
        /// </summary>
        /// <returns>
        /// A new instance of min function.
        /// </returns>
        public static Function CreateMax()
        {
            Function function = new Function();
            function.Name = "max";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a limit(x, xmin, xmax) function.
        /// </summary>
        /// <returns>
        /// A new instance of limit function.
        /// </returns>
        public static Function CreateLimit()
        {
            Function function = new Function();
            function.Name = "limit";
            function.VirtualParameters = false;
            function.ArgumentsCount = 3;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a ln() function.
        /// </summary>
        /// <returns>
        /// A new instance of ln function.
        /// </returns>
        public static Function CreateLn()
        {
            Function function = new Function();
            function.Name = "ln";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a log() function.
        /// </summary>
        /// <returns>
        /// A new instance of log function.
        /// </returns>
        public static Function CreateLog(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "log";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a log10() function.
        /// </summary>
        /// <returns>
        /// A new instance of log10 function.
        /// </returns>
        public static Function CreateLog10(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "log10";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a cbrt() function.
        /// </summary>
        /// <returns>
        /// A new instance of cbrt function.
        /// </returns>
        public static Function CreateCbrt()
        {
            Function function = new Function();
            function.Name = "cbrt";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a buf() function.
        /// </summary>
        /// <returns>
        /// A new instance of buf function.
        /// </returns>
        public static Function CreateBuf()
        {
            Function function = new Function();
            function.Name = "buf";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a ceil() function.
        /// </summary>
        /// <returns>
        /// A new instance of ceil function.
        /// </returns>
        public static Function CreateCeil()
        {
            Function function = new Function();
            function.Name = "ceil";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a abs() function.
        /// </summary>
        /// <returns>
        /// A new instance of abs function.
        /// </returns>
        public static Function CreateAbs()
        {
            Function function = new Function();
            function.Name = "abs";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a floor() function.
        /// </summary>
        /// <returns>
        /// A new instance of floor function.
        /// </returns>
        public static Function CreateFloor()
        {
            Function function = new Function();
            function.Name = "floor";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a hypot() function.
        /// </summary>
        /// <returns>
        /// A new instance of hypot function.
        /// </returns>
        public static Function CreateHypot()
        {
            Function function = new Function();
            function.Name = "hypot";
            function.VirtualParameters = false;
            function.ArgumentsCount = 2;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a int() function.
        /// </summary>
        /// <returns>
        /// A new instance of int function.
        /// </returns>
        public static Function CreateInt()
        {
            Function function = new Function();
            function.Name = "int";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a inv() function.
        /// </summary>
        /// <returns>
        /// A new instance of int function.
        /// </returns>
        public static Function CreateInv()
        {
            Function function = new Function();
            function.Name = "inv";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a exp() function.
        /// </summary>
        /// <returns>
        /// A new instance of exp function.
        /// </returns>
        public static Function CreateExp()
        {
            Function function = new Function();
            function.Name = "exp";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a db() function.
        /// </summary>
        /// <returns>
        /// A new instance of db function.
        /// </returns>
        public static Function CreateDb(SpiceEvaluatorMode mode)
        {
            Function function = new Function();
            function.Name = "db";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a round() function.
        /// </summary>
        /// <returns>
        /// A new instance of round function.
        /// </returns>
        public static Function CreateRound()
        {
            Function function = new Function();
            function.Name = "round";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a u() function.
        /// </summary>
        /// <returns>
        /// A new instance of u function.
        /// </returns>
        public static Function CreateU()
        {
            Function function = new Function();
            function.Name = "u";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a uramp() function.
        /// </summary>
        /// <returns>
        /// A new instance of uramp function.
        /// </returns>
        public static Function CreateURamp()
        {
            Function function = new Function();
            function.Name = "uramp";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
        /// Get a sgn() function.
        /// </summary>
        /// <returns>
        /// A new instance of sgn function.
        /// </returns>
        public static Function CreateSgn()
        {
            Function function = new Function();
            function.Name = "sgn";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;
            function.ReturnType = typeof(double);

            function.DoubleArgsLogic = (image, args, evaluator) =>
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
