using System.Collections.Generic;
using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.Parser.Expressions;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation
{
    /// <summary>
    /// Evalues strings to double
    /// </summary>
    public class SpiceEvaluator : Evaluator, ISpiceEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator()
            : base(new SpiceExpressionParser(), new ExpressionRegistry())
        {
            Parameters.Add("TEMP", Circuit.ReferenceTemperature - Circuit.CelsiusKelvin);

            CustomFunctions.Add("table", TableFunction.Create(this));
            CustomFunctions.Add("random", RandomFunctions.CreateRandom());
            CustomFunctions.Add("min", MathFunctions.CreateMin());
            CustomFunctions.Add("max", MathFunctions.CreateMax());
        }

        public ISpiceEvaluator CreateChildEvaluator()
        {
            var newEvaluator = new SpiceEvaluator();

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
