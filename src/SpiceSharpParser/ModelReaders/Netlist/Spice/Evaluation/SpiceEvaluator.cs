using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    /// <summary>
    /// Spice expressions evaluator.
    /// </summary>
    public class SpiceEvaluator : Evaluator, ISpiceEvaluator
    {
        public SpiceEvaluator()
            : this(string.Empty, null, SpiceEvaluatorMode.LtSpice, null, new ExpressionRegistry(), true)
        {
        }

        public SpiceEvaluator(SpiceEvaluatorMode mode)
            : this(string.Empty, null, mode, null, new ExpressionRegistry(), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(string name, object context, SpiceEvaluatorMode mode, int? seed, ExpressionRegistry registry, bool ignoreCaseFunctionNames)
            : base(name, context, new SpiceExpressionParser(mode == SpiceEvaluatorMode.LtSpice, ignoreCaseFunctionNames), registry, seed)
        {
            IgnoreCaseFunctionNames = ignoreCaseFunctionNames;
            Mode = mode;
            Parameters.Add("TEMP", new ConstantEvaluatorExpression(Circuit.ReferenceTemperature - Circuit.CelsiusKelvin));
            Parameters.Add("TIME", new ConstantEvaluatorExpression(0));
            CreateCustomFunctions();
        }

        public bool IgnoreCaseFunctionNames { get; }

        /// <summary>
        /// Gets or sets the mode of evaluator.
        /// </summary>
        public SpiceEvaluatorMode Mode { get; set; }

        /// <summary>
        /// Creates a child evaluator.
        /// </summary>
        /// <returns>
        /// A child evaluator.
        /// </returns>
        public override IEvaluator CreateChildEvaluator(string name, object context)
        {
            SpiceEvaluator newEvaluator = (SpiceEvaluator)Clone();
            newEvaluator.Name = name;
            newEvaluator.Context = context;
            newEvaluator.Children.Clear();
            newEvaluator.Registry.Invalidate(newEvaluator);

            Children.Add(newEvaluator);
            return newEvaluator;
        }

        public override IEvaluator Clone()
        {
            var clone = new SpiceEvaluator(this.Name, this.Context, this.Mode, this.Seed, this.Registry.Clone(), this.IgnoreCaseFunctionNames);
            clone.Initialize(this.Parameters, this.CustomFunctions, this.Children);
            return clone;
        }

        private void CreateCustomFunctions()
        {
            this.CustomFunctions.Add("**", MathFunctions.CreatePowInfix(Mode));
            this.CustomFunctions.Add("abs", MathFunctions.CreateAbs());
            this.CustomFunctions.Add("buf", MathFunctions.CreateBuf());
            this.CustomFunctions.Add("cbrt", MathFunctions.CreateCbrt());
            this.CustomFunctions.Add("ceil", MathFunctions.CreateCeil());
            this.CustomFunctions.Add("db", MathFunctions.CreateDb(Mode));
            this.CustomFunctions.Add("def", ControlFunctions.CreateDef());
            this.CustomFunctions.Add("exp", MathFunctions.CreateExp());
            this.CustomFunctions.Add("fabs", MathFunctions.CreateAbs());
            this.CustomFunctions.Add("flat", RandomFunctions.CreateFlat());
            this.CustomFunctions.Add("floor", MathFunctions.CreateFloor());
            this.CustomFunctions.Add("gauss", RandomFunctions.CreateGauss());
            this.CustomFunctions.Add("hypot", MathFunctions.CreateHypot());
            this.CustomFunctions.Add("if", ControlFunctions.CreateIf());
            this.CustomFunctions.Add("lazy", ControlFunctions.CreateLazy());
            this.CustomFunctions.Add("int", MathFunctions.CreateInt());
            this.CustomFunctions.Add("inv", MathFunctions.CreateInv());
            this.CustomFunctions.Add("ln", MathFunctions.CreateLn());
            this.CustomFunctions.Add("limit", MathFunctions.CreateLimit());
            this.CustomFunctions.Add("log", MathFunctions.CreateLog(Mode));
            this.CustomFunctions.Add("log10", MathFunctions.CreateLog10(Mode));
            this.CustomFunctions.Add("max", MathFunctions.CreateMax());
            this.CustomFunctions.Add("min", MathFunctions.CreateMin());
            this.CustomFunctions.Add("nint", MathFunctions.CreateRound());
            this.CustomFunctions.Add("pow", MathFunctions.CreatePow(Mode));
            this.CustomFunctions.Add("pwr", MathFunctions.CreatePwr(Mode));
            this.CustomFunctions.Add("pwrs", MathFunctions.CreatePwrs());
            this.CustomFunctions.Add("random", RandomFunctions.CreateRandom());
            this.CustomFunctions.Add("round", MathFunctions.CreateRound());
            this.CustomFunctions.Add("sqrt", MathFunctions.CreateSqrt(Mode));
            this.CustomFunctions.Add("sgn", MathFunctions.CreateSgn());
            this.CustomFunctions.Add("table", TableFunction.Create());
            this.CustomFunctions.Add("u", MathFunctions.CreateU());
            this.CustomFunctions.Add("uramp", MathFunctions.CreateURamp());
        }
    }
}
