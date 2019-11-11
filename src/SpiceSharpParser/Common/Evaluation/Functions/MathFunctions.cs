using SpiceSharpParser.Common.Evaluation.Functions.Math;

namespace SpiceSharpParser.Common.Evaluation.Functions
{
    public class MathFunctions
    {
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
