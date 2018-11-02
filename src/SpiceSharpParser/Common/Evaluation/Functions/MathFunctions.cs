using System;

namespace SpiceSharpParser.Common.Evaluation.Functions
{
    public class MathFunctions
    {
        /// <summary>
        /// Get a cos() function.
        /// </summary>
        /// <returns>
        /// A new instance of cos() function.
        /// </returns>
        public static Function CreateCos()
        {
            Function function = new Function();
            function.Name = "cos";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double d = args[0];

                return Math.Cos(d);
            };

            return function;
        }

        /// <summary>
        /// Get a sin() function.
        /// </summary>
        /// <returns>
        /// A new instance of sin() function.
        /// </returns>
        public static Function CreateSin()
        {
            Function function = new Function();
            function.Name = "sin";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double d = args[0];

                return Math.Sin(d);
            };

            return function;
        }

        /// <summary>
        /// Get a tan() function.
        /// </summary>
        /// <returns>
        /// A new instance of tan() function.
        /// </returns>
        public static Function CreateTan()
        {
            Function function = new Function();
            function.Name = "tan";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Tan(a);
            };

            return function;
        }

        /// <summary>
        /// Get a cosh() function.
        /// </summary>
        /// <returns>
        /// A new instance of cosh() function.
        /// </returns>
        public static Function CreateCosh()
        {
            Function function = new Function();
            function.Name = "cosh";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Cosh(a);
            };

            return function;
        }

        /// <summary>
        /// Get a sinh() function.
        /// </summary>
        /// <returns>
        /// A new instance of sinh() function.
        /// </returns>
        public static Function CreateSinh()
        {
            Function function = new Function();
            function.Name = "sinh";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Sinh(a);
            };

            return function;
        }

        /// <summary>
        /// Get a tanh() function.
        /// </summary>
        /// <returns>
        /// A new instance of tanh() function.
        /// </returns>
        public static Function CreateTanh()
        {
            Function function = new Function();
            function.Name = "tanh";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Tanh(a);
            };

            return function;
        }

        /// <summary>
        /// Get a acos() function.
        /// </summary>
        /// <returns>
        /// A new instance of acos() function.
        /// </returns>
        public static Function CreateACos()
        {
            Function function = new Function();
            function.Name = "acos";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Acos(a);
            };

            return function;
        }

        /// <summary>
        /// Get a asin() function.
        /// </summary>
        /// <returns>
        /// A new instance of asin() function.
        /// </returns>
        public static Function CreateASin()
        {
            Function function = new Function();
            function.Name = "asin";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Asin(a);
            };

            return function;
        }

        /// <summary>
        /// Get a atan() function.
        /// </summary>
        /// <returns>
        /// A new instance of atan() function.
        /// </returns>
        public static Function CreateATan()
        {
            Function function = new Function();
            function.Name = "atan";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double a = args[0];
                return Math.Atan(a);
            };

            return function;
        }

        /// <summary>
        /// Get a atan() function.
        /// </summary>
        /// <returns>
        /// A new instance of atan() function.
        /// </returns>
        public static Function CreateATan2()
        {
            Function function = new Function();
            function.Name = "atan2";
            function.VirtualParameters = false;
            function.ArgumentsCount = 1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
            {
                double y = args[0];
                double x = args[1];

                return Math.Atan2(y, x);
            };

            return function;
        }
    }
}
