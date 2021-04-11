using SpiceSharpParser.Common.Evaluation;
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
        /// Get a pwr() function.
        /// </summary>
        /// <param name="mode">Evaluator mode.</param>
        /// <returns>
        /// A new instance of pwr function.
        /// </returns>
        public static IFunction<double, double> CreatePwr(SpiceExpressionMode mode)
        {
            return new PwrFunction(mode);
        }

        /// <summary>
        /// Get a pwrs() function.
        /// </summary>
        /// <returns>
        /// A new instance of pwrs function.
        /// </returns>
        public static IFunction<double, double> CreatePwrs()
        {
            return new PwrsFunction();
        }

        /// <summary>
        /// Get a min() function.
        /// </summary>
        /// <returns>
        /// A new instance of min function.
        /// </returns>
        public static IFunction<double, double> CreateMin()
        {
            return new MinFunction();
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
        /// Get a max() function.
        /// </summary>
        /// <returns>
        /// A new instance of max function.
        /// </returns>
        public static IFunction<double, double> CreateMax()
        {
            return new MaxFunction();
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
        /// Get a ln() function.
        /// </summary>
        /// <returns>
        /// A new instance of ln function.
        /// </returns>
        public static IFunction<double, double> CreateLn()
        {
            return new LnFunction();
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
        /// Get a ceil() function.
        /// </summary>
        /// <returns>
        /// A new instance of ceil function.
        /// </returns>
        public static IFunction<double, double> CreateCeil()
        {
            return new CeilFunction();
        }

        /// <summary>
        /// Get a floor() function.
        /// </summary>
        /// <returns>
        /// A new instance of floor function.
        /// </returns>
        public static IFunction<double, double> CreateFloor()
        {
            return new FloorFunction();
        }

        /// <summary>
        /// Get a hypot() function.
        /// </summary>
        /// <returns>
        /// A new instance of hypot function.
        /// </returns>
        public static IFunction<double, double> CreateHypot()
        {
            return new HypotFunction();
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
        /// Get a db() function.
        /// </summary>
        /// <returns>
        /// A new instance of db function.
        /// </returns>
        public static IFunction<double, double> CreateDb(SpiceExpressionMode mode)
        {
            return new DbFunction(mode);
        }

        /// <summary>
        /// Get a round() function.
        /// </summary>
        /// <returns>
        /// A new instance of round function.
        /// </returns>
        public static IFunction<double, double> CreateRound()
        {
            return new RoundFunction();
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
        /// Get a sgn() function.
        /// </summary>
        /// <returns>
        /// A new instance of sgn function.
        /// </returns>
        public static IFunction<double, double> CreateSgn()
        {
            return new SgnFunction();
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