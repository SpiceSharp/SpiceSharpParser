using System;
using SpiceSharp;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class SpiceExpressionContext : ExpressionContext
    {
        public SpiceExpressionContext(SpiceExpressionMode mode)
            : this(string.Empty, mode, false, false, false)
        {
        }

        public SpiceExpressionContext(
            string name,
            SpiceExpressionMode mode,
            bool isParameterNameCaseSensitive,
            bool isFunctionNameCaseSensitive,
            bool isExpressionNameCaseSensitive)

        : base(name, isParameterNameCaseSensitive, isFunctionNameCaseSensitive, isExpressionNameCaseSensitive)
        {
            this.Mode = mode;
            this.CreateSpiceFunctions();
            this.CreateSpiceParameters();
        }

        public SpiceExpressionMode Mode { get; }

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
            Parameters.Add("FREQ", new ConstantExpression(0));
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
