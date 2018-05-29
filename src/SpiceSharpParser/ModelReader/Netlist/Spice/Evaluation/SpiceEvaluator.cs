using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.Parser.Expressions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation
{
    /// <summary>
    /// Spice expressions evaluator.
    /// </summary>
    public class SpiceEvaluator : Evaluator, ISpiceEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(SpiceEvaluatorMode mode = SpiceEvaluatorMode.Spice3f5)
            : base(new SpiceExpressionParser(mode == SpiceEvaluatorMode.LtSpice), new ExpressionRegistry())
        {
            Mode = mode;
            Parameters.Add("TEMP", Circuit.ReferenceTemperature - Circuit.CelsiusKelvin);

            CustomFunctions.Add("**", MathFunctions.CreatePowInfix(mode));
            CustomFunctions.Add("abs", MathFunctions.CreateAbs());
            CustomFunctions.Add("buf", MathFunctions.CreateBuf());
            CustomFunctions.Add("cbrt", MathFunctions.CreateCbrt());
            CustomFunctions.Add("ceil", MathFunctions.CreateCeil());
            CustomFunctions.Add("db", MathFunctions.CreateDb(mode));
            CustomFunctions.Add("def", ControlFunctions.CreateDef(this));
            CustomFunctions.Add("exp", MathFunctions.CreateExp());
            CustomFunctions.Add("fabs", MathFunctions.CreateAbs());
            CustomFunctions.Add("flat", RandomFunctions.CreateFlat());
            CustomFunctions.Add("floor", MathFunctions.CreateFloor());
            CustomFunctions.Add("hypot", MathFunctions.CreateHypot());
            CustomFunctions.Add("if", MathFunctions.CreateIf());
            CustomFunctions.Add("int", MathFunctions.CreateInt());
            CustomFunctions.Add("inv", MathFunctions.CreateInv());
            CustomFunctions.Add("ln", MathFunctions.CreateLn());
            CustomFunctions.Add("log", MathFunctions.CreateLog(mode));
            CustomFunctions.Add("log10", MathFunctions.CreateLog10(mode));
            CustomFunctions.Add("max", MathFunctions.CreateMax());
            CustomFunctions.Add("min", MathFunctions.CreateMin());
            CustomFunctions.Add("nint", MathFunctions.CreateRound());
            CustomFunctions.Add("pow", MathFunctions.CreatePow(mode));
            CustomFunctions.Add("pwr", MathFunctions.CreatePwr(mode));
            CustomFunctions.Add("pwrs", MathFunctions.CreatePwrs());
            CustomFunctions.Add("random", RandomFunctions.CreateRandom());
            CustomFunctions.Add("round", MathFunctions.CreateRound());
            CustomFunctions.Add("sqrt", MathFunctions.CreateSqrt(mode));
            CustomFunctions.Add("sgn", MathFunctions.CreateSgn());
            CustomFunctions.Add("table", TableFunction.Create(this));
            CustomFunctions.Add("u", MathFunctions.CreateU());
            CustomFunctions.Add("uramp", MathFunctions.CreateURamp());
        }

        /// <summary>
        /// Gets the mode of evaluator.
        /// </summary>
        public SpiceEvaluatorMode Mode { get; }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A new evaluator.
        /// </returns>
        public ISpiceEvaluator CreateChildEvaluator()
        {
            var newEvaluator = new SpiceEvaluator(Mode);

            foreach (var parameterName in this.GetParameterNames())
            {
                newEvaluator.Parameters[parameterName] = this.GetParameterValue(parameterName);
            }

            foreach (var customFunction in CustomFunctions)
            {
                newEvaluator.CustomFunctions[customFunction.Key] = customFunction.Value;
            }

            return newEvaluator;
        }
    }
}
