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
            : this(string.Empty, null, new SpiceExpressionParser(true), SpiceEvaluatorMode.Spice3f5, null, new ExpressionRegistry(false, false), false, false)
        {
        }

        public SpiceEvaluator(SpiceEvaluatorMode mode)
            : this(string.Empty, null, new SpiceExpressionParser(mode == SpiceEvaluatorMode.LtSpice), mode, null, new ExpressionRegistry(false, false), false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(string name, object context, SpiceEvaluatorMode mode, int? seed, ExpressionRegistry registry, bool isFunctionNameCaseSensitive, bool isParameterNameCaseSensitive)
            : this(name, context, new SpiceExpressionParser(mode == SpiceEvaluatorMode.LtSpice), mode, seed, registry, isFunctionNameCaseSensitive, isParameterNameCaseSensitive)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceEvaluator"/> class.
        /// </summary>
        public SpiceEvaluator(string name, object context, IExpressionParser expressionParser, SpiceEvaluatorMode mode, int? seed, ExpressionRegistry registry, bool isFunctionNameCaseSensitive, bool isParameterNameCaseSensitive)
            : base(name, context, expressionParser, seed, registry, isFunctionNameCaseSensitive, isParameterNameCaseSensitive)
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

        /// <summary>
        /// Clones the evaluator.
        /// </summary>
        /// <param name="deep">Specifies whether cloning is deep.</param>
        /// <returns>
        /// A clone of evaluator.
        /// </returns>
        public override IEvaluator Clone(bool deep)
        {
            var clone = new SpiceEvaluator(Name, Context, Mode, Seed, Registry.Clone(), IsFunctionNameCaseSensitive, IsParameterNameCaseSensitive);
            clone.Initialize(
                Parameters, 
                Functions,
                deep ? Children : new System.Collections.Generic.List<IEvaluator>(), 
                ParseResults);
            return clone;
        }

        private void CreateSpiceFunctions()
        {
            Functions.Add("**", MathFunctions.CreatePowInfix(Mode));
            Functions.Add("abs", MathFunctions.CreateAbs());
            Functions.Add("buf", MathFunctions.CreateBuf());
            Functions.Add("cbrt", MathFunctions.CreateCbrt());
            Functions.Add("ceil", MathFunctions.CreateCeil());
            Functions.Add("db", MathFunctions.CreateDb(Mode));
            Functions.Add("def", ControlFunctions.CreateDef());
            Functions.Add("exp", MathFunctions.CreateExp());
            Functions.Add("fabs", MathFunctions.CreateAbs());
            Functions.Add("flat", RandomFunctions.CreateFlat());
            Functions.Add("floor", MathFunctions.CreateFloor());
            Functions.Add("gauss", RandomFunctions.CreateGauss());
            Functions.Add("hypot", MathFunctions.CreateHypot());
            Functions.Add("if", ControlFunctions.CreateIf());
            Functions.Add("lazy", ControlFunctions.CreateLazy());
            Functions.Add("int", MathFunctions.CreateInt());
            Functions.Add("inv", MathFunctions.CreateInv());
            Functions.Add("ln", MathFunctions.CreateLn());
            Functions.Add("limit", MathFunctions.CreateLimit());
            Functions.Add("log", MathFunctions.CreateLog(Mode));
            Functions.Add("log10", MathFunctions.CreateLog10(Mode));
            Functions.Add("max", MathFunctions.CreateMax());
            Functions.Add("min", MathFunctions.CreateMin());
            Functions.Add("nint", MathFunctions.CreateRound());
            Functions.Add("pow", MathFunctions.CreatePow(Mode));
            Functions.Add("pwr", MathFunctions.CreatePwr(Mode));
            Functions.Add("pwrs", MathFunctions.CreatePwrs());
            Functions.Add("random", RandomFunctions.CreateRandom());
            Functions.Add("round", MathFunctions.CreateRound());
            Functions.Add("sqrt", MathFunctions.CreateSqrt(Mode));
            Functions.Add("sgn", MathFunctions.CreateSgn());
            Functions.Add("table", TableFunction.Create());
            Functions.Add("u", MathFunctions.CreateU());
            Functions.Add("uramp", MathFunctions.CreateURamp());
            Functions.Add("poly", PolyFunction.Create());
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
