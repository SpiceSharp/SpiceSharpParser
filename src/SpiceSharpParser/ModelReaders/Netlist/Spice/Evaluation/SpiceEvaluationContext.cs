using SpiceSharp;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;
using SpiceSharpParser.Parsers.Expression;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class SpiceEvaluationContext : EvaluationContext
    {
        public SpiceEvaluationContext(
            string name,
            SpiceExpressionMode mode,
            ISpiceNetlistCaseSensitivitySettings caseSetting,
            IRandomizer randomizer,
            IExpressionParserFactory expressionParserFactory,
            IExpressionFeaturesReader expressionFeaturesReader,
            IExpressionValueProvider expressionValueProvider,
            INameGenerator nameGenerator,
            IResultService resultService)

        : base(
              name,
              caseSetting,
              randomizer,
              expressionParserFactory,
              expressionFeaturesReader,
              expressionValueProvider,
              nameGenerator,
              resultService)
        {
            Mode = mode;
            CreateSpiceFunctions();
            CreateSpiceParameters();
        }

        public SpiceExpressionMode Mode { get; }

        private void CreateSpiceFunctions()
        {
            var functions = new List<IFunction>
            {
                MathFunctions.CreatePos(),
                RandomFunctions.CreateAGauss(),
                RandomFunctions.CreateAUnif(),
                MathFunctions.CreateBuf(),
                MathFunctions.CreateCbrt(),
                MathFunctions.CreateCeil(),
                MathFunctions.CreateDb(Mode),
                ControlFunctions.CreateDef(),
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
                MathFunctions.CreateMax(),
                RandomFunctions.CreateMc(),
                MathFunctions.CreateMin(),
                MathFunctions.CreateNint(),
                MathFunctions.CreateRound(),
                MathFunctions.CreatePwr(Mode),
                MathFunctions.CreatePwrs(),
                RandomFunctions.CreateRandom(),
                MathFunctions.CreateSgn(),
                MathFunctions.CreateU(),
                RandomFunctions.CreateUnif(),
                MathFunctions.CreateURamp(),
                MathFunctions.CreatePoly(),
            };

            foreach (var function in functions)
            {
                AddFunction(function.Name,  null, null, function);
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