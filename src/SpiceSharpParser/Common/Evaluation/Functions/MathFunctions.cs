using SpiceSharpParser.Common.Evaluation.Functions.Math;

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
        public static IFunction<double, double> CreateCos()
        {
            return new CosFunction();
        }

        /// <summary>
        /// Get a sin() function.
        /// </summary>
        /// <returns>
        /// A new instance of sin() function.
        /// </returns>
        public static IFunction<double, double> CreateSin()
        {
            return new SinFunction();
        }

        /// <summary>
        /// Get a tan() function.
        /// </summary>
        /// <returns>
        /// A new instance of tan() function.
        /// </returns>
        public static IFunction<double, double> CreateTan()
        {
            return new TanFunction();
        }

        /// <summary>
        /// Get a cosh() function.
        /// </summary>
        /// <returns>
        /// A new instance of cosh() function.
        /// </returns>
        public static IFunction<double, double> CreateCosh()
        {
            return new CoshFunction();
        }

        /// <summary>
        /// Get a sinh() function.
        /// </summary>
        /// <returns>
        /// A new instance of sinh() function.
        /// </returns>
        public static IFunction<double, double> CreateSinh()
        {
            return new SinhFunction();
        }

        /// <summary>
        /// Get a tanh() function.
        /// </summary>
        /// <returns>
        /// A new instance of tanh() function.
        /// </returns>
        public static IFunction<double, double> CreateTanh()
        {
            return new TanhFunction();
        }

        /// <summary>
        /// Get a acos() function.
        /// </summary>
        /// <returns>
        /// A new instance of acos() function.
        /// </returns>
        public static IFunction<double, double> CreateACos()
        {
            return new ACosFunction();
        }

        /// <summary>
        /// Get a asin() function.
        /// </summary>
        /// <returns>
        /// A new instance of asin() function.
        /// </returns>
        public static IFunction<double, double> CreateASin()
        {
            return new ASinFunction();
        }

        /// <summary>
        /// Get a atan() function.
        /// </summary>
        /// <returns>
        /// A new instance of atan() function.
        /// </returns>
        public static IFunction<double, double> CreateATan()
        {
            return new ATanFunction();
        }

        /// <summary>
        /// Get a atan() function.
        /// </summary>
        /// <returns>
        /// A new instance of atan() function.
        /// </returns>
        public static IFunction<double, double> CreateATan2()
        {
            return new ATan2Function();
        }
    }
}
