using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries;
using SpiceSharpParser.Parsers.Expression;
using System.Globalization;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation
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

            Parameters.Add("TEMP", new LazyExpression((e, c) => (Circuit.ReferenceTemperature - Circuit.CelsiusKelvin), "temp"));

            CustomFunctions.Add("**", MathFunctions.CreatePowInfix(Mode));
            CustomFunctions.Add("abs", MathFunctions.CreateAbs());
            CustomFunctions.Add("buf", MathFunctions.CreateBuf());
            CustomFunctions.Add("cbrt", MathFunctions.CreateCbrt());
            CustomFunctions.Add("ceil", MathFunctions.CreateCeil());
            CustomFunctions.Add("db", MathFunctions.CreateDb(Mode));
            CustomFunctions.Add("def", ControlFunctions.CreateDef());
            CustomFunctions.Add("exp", MathFunctions.CreateExp());
            CustomFunctions.Add("fabs", MathFunctions.CreateAbs());
            CustomFunctions.Add("flat", RandomFunctions.CreateFlat());
            CustomFunctions.Add("floor", MathFunctions.CreateFloor());
            CustomFunctions.Add("hypot", MathFunctions.CreateHypot());
            CustomFunctions.Add("if", ControlFunctions.CreateIf());
            CustomFunctions.Add("lazy", ControlFunctions.CreateLazy());
            CustomFunctions.Add("int", MathFunctions.CreateInt());
            CustomFunctions.Add("inv", MathFunctions.CreateInv());
            CustomFunctions.Add("ln", MathFunctions.CreateLn());
            CustomFunctions.Add("log", MathFunctions.CreateLog(Mode));
            CustomFunctions.Add("log10", MathFunctions.CreateLog10(Mode));
            CustomFunctions.Add("max", MathFunctions.CreateMax());
            CustomFunctions.Add("min", MathFunctions.CreateMin());
            CustomFunctions.Add("nint", MathFunctions.CreateRound());
            CustomFunctions.Add("pow", MathFunctions.CreatePow(Mode));
            CustomFunctions.Add("pwr", MathFunctions.CreatePwr(Mode));
            CustomFunctions.Add("pwrs", MathFunctions.CreatePwrs());
            CustomFunctions.Add("random", RandomFunctions.CreateRandom());
            CustomFunctions.Add("round", MathFunctions.CreateRound());
            CustomFunctions.Add("sqrt", MathFunctions.CreateSqrt(Mode));
            CustomFunctions.Add("sgn", MathFunctions.CreateSgn());
            CustomFunctions.Add("table", TableFunction.Create());
            CustomFunctions.Add("u", MathFunctions.CreateU());
            CustomFunctions.Add("uramp", MathFunctions.CreateURamp());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(SpiceEvaluatorMode mode, IExporterRegistry exporters, INodeNameGenerator nodeNameGenerator, IObjectNameGenerator objectNameGenerator)
            : this(mode)
        {
            ExportFunctions.Add(CustomFunctions, exporters, nodeNameGenerator, objectNameGenerator);
        }

        /// <summary>
        /// Gets the mode of evaluator.
        /// </summary>
        public SpiceEvaluatorMode Mode { get; }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A child evaluator.
        /// </returns>
        public override IEvaluator CreateChildEvaluator()
        {
            var newEvaluator = new SpiceEvaluator(Mode);

            foreach (var parameterName in this.GetParameterNames())
            {
                newEvaluator.Parameters[parameterName] = this.ExpressionParser.Parameters[parameterName];
            }

            foreach (var customFunction in CustomFunctions)
            {
                newEvaluator.CustomFunctions[customFunction.Key] = customFunction.Value;
            }

            return newEvaluator;
        }
    }
}
