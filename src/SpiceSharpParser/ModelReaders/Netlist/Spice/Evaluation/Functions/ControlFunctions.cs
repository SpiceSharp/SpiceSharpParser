using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Control;

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
        public static IFunction<double, double> CreateDef()
        {
            return new DefFunction();
        }

        /// <summary>
        /// Get a if() function.
        /// </summary>
        /// <returns>
        /// A new instance of if function.
        /// </returns>
        public static IFunction<double, double> CreateIf()
        {
            return new IfFunction();
        }
    }
}