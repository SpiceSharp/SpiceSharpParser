using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Random;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class RandomFunctions
    {
        /// <summary>
        /// Get a mc() function.
        /// </summary>
        /// <returns>
        /// A new instance of random mc function.
        /// </returns>
        public static IFunction<double, double> CreateMc()
        {
            return new McFunction();
        }

        /// <summary>
        /// Get a gauss() function.
        /// </summary>
        /// <returns>
        /// A new instance of random gauss function.
        /// </returns>
        public static IFunction<double, double> CreateGauss()
        {
            return new GaussFunction();
        }

        /// <summary>
        /// Get a random() function. It generates number between 0.0 and 1.0 (uniform distribution).
        /// </summary>
        /// <returns>
        /// A new instance of random function.
        /// </returns>
        public static IFunction<double, double> CreateRandom()
        {
            return new RandomFunction();
        }

        /// <summary>
        /// Get a flat() function. It generates number between -x and +x.
        /// </summary>
        /// <returns>
        /// A new instance of random function.
        /// </returns>
        public static IFunction<double, double> CreateFlat()
        {
            return new FlatFunction();
        }
    }
}
