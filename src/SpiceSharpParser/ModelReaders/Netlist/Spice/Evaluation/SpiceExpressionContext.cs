using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class SpiceExpressionContext : ExpressionContext
    {
        public SpiceExpressionContext(SpiceExpressionMode mode)
            : this(string.Empty, mode, false, false, false, new Randomizer())
        {
        }

        public SpiceExpressionContext(
            string name,
            SpiceExpressionMode mode,
            bool isParameterNameCaseSensitive,
            bool isFunctionNameCaseSensitive,
            bool isExpressionNameCaseSensitive,
            IRandomizer randomizer)

        : base(name, isParameterNameCaseSensitive, isFunctionNameCaseSensitive, isExpressionNameCaseSensitive, randomizer)
        {
            this.Mode = mode;
            this.CreateCommonFunctions();
            this.CreateSpiceFunctions();
            this.CreateSpiceParameters();
        }

        public SpiceExpressionMode Mode { get; }

        private void CreateSpiceFunctions()
        {
            var functions = new List<IFunction>
            {
                MathFunctions.CreatePos(),
                MathFunctions.CreatePowInfix(Mode),
                MathFunctions.CreateAbs(),
                RandomFunctions.CreateAGauss(),
                RandomFunctions.CreateAUnif(),
                MathFunctions.CreateBuf(),
                MathFunctions.CreateCbrt(),
                MathFunctions.CreateCeil(),
                MathFunctions.CreateDb(Mode),
                ControlFunctions.CreateDef(),
                MathFunctions.CreateExp(),
                MathFunctions.CreateFAbs(),
                RandomFunctions.CreateFlat(),
                MathFunctions.CreateFloor(),
                RandomFunctions.CreateGauss(),
                RandomFunctions.CreateExtendedGauss(),
                MathFunctions.CreateHypot(),
                ControlFunctions.CreateIf(),
                MathFunctions.CreateInt(),
                MathFunctions.CreateInv(),
                MathFunctions.CreateLn(),
                MathFunctions.CreateLimit(),
                RandomFunctions.CreateLimit(),
                MathFunctions.CreateLog(Mode),
                MathFunctions.CreateLog10(Mode),
                MathFunctions.CreateMax(),
                RandomFunctions.CreateMc(),
                MathFunctions.CreateMin(),
                MathFunctions.CreateNint(),
                MathFunctions.CreateRound(),
                MathFunctions.CreatePow(Mode),
                MathFunctions.CreatePwr(Mode),
                MathFunctions.CreatePwrs(),
                RandomFunctions.CreateRandom(),
                MathFunctions.CreateSqrt(Mode),
                MathFunctions.CreateSgn(),
                MathFunctions.CreateTable(),
                MathFunctions.CreateU(),
                RandomFunctions.CreateUnif(),
                MathFunctions.CreateURamp(),
                MathFunctions.CreatePoly()
            };

            foreach (var function in functions)
            {
               AddFunction(function.Name, function);
            }
        }

        private void CreateSpiceParameters()
        {
            Parameters.Add("TEMP", new ConstantExpression(Constants.ReferenceTemperature - Constants.CelsiusKelvin));
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
