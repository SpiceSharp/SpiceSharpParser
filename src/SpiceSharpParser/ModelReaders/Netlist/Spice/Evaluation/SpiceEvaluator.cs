using System;
using SpiceSharp;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    /// <summary>
    /// Spice expressions evaluator.
    /// </summary>
    public class SpiceEvaluator : Evaluator
    {
        public SpiceEvaluator()
            : this(string.Empty, null, SpiceEvaluatorMode.LtSpice, null, new ExpressionRegistry(false, false), false, false)
        {
        }

        public SpiceEvaluator(SpiceEvaluatorMode mode)
            : this(string.Empty, null, mode, null, new ExpressionRegistry(false, false), false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(string name, object context, SpiceEvaluatorMode mode, int? seed, ExpressionRegistry registry, bool isFunctionNameCaseSensitive, bool isParameterNameCaseSensitive)
            : base(name, context, new SpiceExpressionParser(mode == SpiceEvaluatorMode.LtSpice), registry, seed, isFunctionNameCaseSensitive, isParameterNameCaseSensitive)
        {
            Mode = mode;
            CreateSpiceParameters();
            CreateSpiceFunctions();
        }

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
            SpiceEvaluator newEvaluator = (SpiceEvaluator)Clone(false);
            newEvaluator.Name = name;
            newEvaluator.Context = context;

            Children.Add(newEvaluator);
            return newEvaluator;
        }

        public override IEvaluator Clone(bool deep)
        {
            var clone = new SpiceEvaluator(Name, Context, Mode, Seed, Registry.Clone(), IsFunctionNameCaseSensitive, IsParameterNameCaseSensitive);
            clone.Initialize(Parameters, Functions, deep ? Children : new System.Collections.Generic.List<IEvaluator>());
            return clone;
        }

        private void CreateSpiceFunctions()
        {
            this.Functions.Add("**", MathFunctions.CreatePowInfix(Mode));
            this.Functions.Add("abs", MathFunctions.CreateAbs());
            this.Functions.Add("buf", MathFunctions.CreateBuf());
            this.Functions.Add("cbrt", MathFunctions.CreateCbrt());
            this.Functions.Add("ceil", MathFunctions.CreateCeil());
            this.Functions.Add("db", MathFunctions.CreateDb(Mode));
            this.Functions.Add("def", ControlFunctions.CreateDef());
            this.Functions.Add("exp", MathFunctions.CreateExp());
            this.Functions.Add("fabs", MathFunctions.CreateAbs());
            this.Functions.Add("flat", RandomFunctions.CreateFlat());
            this.Functions.Add("floor", MathFunctions.CreateFloor());
            this.Functions.Add("gauss", RandomFunctions.CreateGauss());
            this.Functions.Add("hypot", MathFunctions.CreateHypot());
            this.Functions.Add("if", ControlFunctions.CreateIf());
            this.Functions.Add("lazy", ControlFunctions.CreateLazy());
            this.Functions.Add("int", MathFunctions.CreateInt());
            this.Functions.Add("inv", MathFunctions.CreateInv());
            this.Functions.Add("ln", MathFunctions.CreateLn());
            this.Functions.Add("limit", MathFunctions.CreateLimit());
            this.Functions.Add("log", MathFunctions.CreateLog(Mode));
            this.Functions.Add("log10", MathFunctions.CreateLog10(Mode));
            this.Functions.Add("max", MathFunctions.CreateMax());
            this.Functions.Add("min", MathFunctions.CreateMin());
            this.Functions.Add("nint", MathFunctions.CreateRound());
            this.Functions.Add("pow", MathFunctions.CreatePow(Mode));
            this.Functions.Add("pwr", MathFunctions.CreatePwr(Mode));
            this.Functions.Add("pwrs", MathFunctions.CreatePwrs());
            this.Functions.Add("random", RandomFunctions.CreateRandom());
            this.Functions.Add("round", MathFunctions.CreateRound());
            this.Functions.Add("sqrt", MathFunctions.CreateSqrt(Mode));
            this.Functions.Add("sgn", MathFunctions.CreateSgn());
            this.Functions.Add("table", TableFunction.Create());
            this.Functions.Add("u", MathFunctions.CreateU());
            this.Functions.Add("uramp", MathFunctions.CreateURamp());
        }

        private void CreateSpiceParameters()
        {
            Parameters.Add("TEMP", new ConstantExpression(Circuit.ReferenceTemperature - Circuit.CelsiusKelvin));
            Parameters.Add("TIME", new ConstantExpression(0));
            Parameters.Add("PI", new ConstantExpression(Math.PI));
            Parameters.Add("E", new ConstantExpression(Math.E));
            Parameters.Add("false", new ConstantExpression(0));
            Parameters.Add("true", new ConstantExpression(1));
            Parameters.Add("yes", new ConstantExpression(1));
            Parameters.Add("no", new ConstantExpression(0));
            Parameters.Add("kelvin", new ConstantExpression(-273.15));
            Parameters.Add("echarge", new ConstantExpression(1.60219e-19));
            Parameters.Add("c", new ConstantExpression(299792500));
            Parameters.Add("boltz", new ConstantExpression(1.38062e-23));
            Parameters.Add("NaN", new ConstantExpression(double.NaN));
        }
    }
}
