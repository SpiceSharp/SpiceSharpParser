using SpiceSharp;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class SpiceEvaluationContext : EvaluationContext
    {
        public SpiceEvaluationContext(
            string name,
            SpiceNetlistCaseSensitivitySettings caseSetting,
            IRandomizer randomizer,
            IExpressionParserFactory expressionParserFactory,
            IExpressionFeaturesReader expressionFeaturesReader,
            INameGenerator nameGenerator)

        : base(
              name,
              caseSetting,
              randomizer,
              expressionParserFactory,
              expressionFeaturesReader,
              nameGenerator)
        {
            CreateSpiceFunctions();
            CreateSpiceParameters();
        }

        private void CreateSpiceFunctions()
        {
            var functions = new List<IFunction>
            {
                MathFunctions.CreatePos(),
                RandomFunctions.CreateAGauss(),
                RandomFunctions.CreateAUnif(),
                MathFunctions.CreateBuf(),
                MathFunctions.CreateCbrt(),
                ControlFunctions.CreateDef(),
                RandomFunctions.CreateFlat(),
                RandomFunctions.CreateGauss(),
                RandomFunctions.CreateExtendedGauss(),
                ControlFunctions.CreateIf(),
                MathFunctions.CreateInt(),
                MathFunctions.CreateInv(),
                MathFunctions.CreateLimit(),
                RandomFunctions.CreateLimit(),
                RandomFunctions.CreateMc(),
                MathFunctions.CreateNint(),
                RandomFunctions.CreateRandom(),
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