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

            CustomFunctions.Add("table", TableFunction.Create(this));
            CustomFunctions.Add("random", RandomFunctions.CreateRandom());
            CustomFunctions.Add("min", MathFunctions.CreateMin());
            CustomFunctions.Add("max", MathFunctions.CreateMax());
            CustomFunctions.Add("pow", MathFunctions.CreatePow(mode));
            CustomFunctions.Add("**", MathFunctions.CreatePowInfix(mode));
            CustomFunctions.Add("sqrt", MathFunctions.CreateSqrt(mode));

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
