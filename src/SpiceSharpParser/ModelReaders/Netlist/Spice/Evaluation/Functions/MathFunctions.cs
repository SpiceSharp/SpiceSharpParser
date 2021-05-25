using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public static class MathFunctions
    {
        /// <summary>
        /// Get a pos() function.
        /// </summary>
        /// <returns>
        /// A new instance of poly function.
        /// </returns>
        public static IFunction<double, double> CreatePos()
        {
            return new PosFunction();
        }

        /// <summary>
        /// Get a poly() function.
        /// </summary>
        /// <returns>
        /// A new instance of poly function.
        /// </returns>
        public static IFunction<double, double> CreatePoly()
        {
            return new PolyFunction();
        }

        /// <summary>
        /// Get a nint() function.
        /// </summary>
        /// <returns>
        /// A new instance of min function.
        /// </returns>
        public static IFunction<double, double> CreateNint()
        {
            return new NintFunction();
        }

        /// <summary>
        /// Get a limit(x, xmin, xmax) function.
        /// </summary>
        /// <returns>
        /// A new instance of limit function.
        /// </returns>
        public static IFunction<double, double> CreateLimit()
        {
            return new LimitFunction();
        }

        /// <summary>
        /// Get a cbrt() function.
        /// </summary>
        /// <returns>
        /// A new instance of cbrt function.
        /// </returns>
        public static IFunction<double, double> CreateCbrt()
        {
            return new CbrtFunction();
        }

        /// <summary>
        /// Get a buf() function.
        /// </summary>
        /// <returns>
        /// A new instance of buf function.
        /// </returns>
        public static IFunction<double, double> CreateBuf()
        {
            return new BufFunction();
        }

        /// <summary>
        /// Get a int() function.
        /// </summary>
        /// <returns>
        /// A new instance of int function.
        /// </returns>
        public static IFunction<double, double> CreateInt()
        {
            return new IntFunction();
        }

        /// <summary>
        /// Get a inv() function.
        /// </summary>
        /// <returns>
        /// A new instance of int function.
        /// </returns>
        public static IFunction<double, double> CreateInv()
        {
            return new InvFunction();
        }

        /// <summary>
        /// Get a u() function.
        /// </summary>
        /// <returns>
        /// A new instance of u function.
        /// </returns>
        public static IFunction<double, double> CreateU()
        {
            return new UFunction();
        }

        /// <summary>
        /// Get a uramp() function.
        /// </summary>
        /// <returns>
        /// A new instance of uramp function.
        /// </returns>
        public static IFunction<double, double> CreateURamp()
        {
            return new URampFunction();
        }

        /// <summary>
        /// Get a table() function.
        /// </summary>
        /// <returns>
        /// A new instance of a table function.
        /// </returns>
        public static IFunction<double, double> CreateTable()
        {
            return null; // TODO
        }
    }
}