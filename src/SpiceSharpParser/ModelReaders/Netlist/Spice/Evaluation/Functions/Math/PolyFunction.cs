using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PolyFunction : Function<double, double>
    {
        public PolyFunction()
        {
            Name = "poly";
            ArgumentsCount = -1;
        }

        public override double Logic(string image, double[] args, IEvaluator evaluator, ExpressionContext context)
        {
            var dimension = (int)args[0];
            List<double> variables = new List<double>();

            if (args.Length < dimension + 1)
            {
                throw new Exception("Too less variables for poly");
            }

            for (var i = 1; i <= dimension; i++)
            {
                variables.Add(args[i]);
            }

            List<double> coefficients = new List<double>();

            for (var i = dimension + 1; i < args.Length; i++)
            {
                coefficients.Add(args[i]);
            }

            if (coefficients.Count == 0)
            {
                return 0;
            }

            var combinations = CombinationCache.GetCombinations(coefficients.Count, dimension);
            double sum = 0.0;
            sum += coefficients[0];

            for (int i = 1; i < combinations.Count; i++)
            {
                sum += ComputeSumElementValue(variables, coefficients[i], combinations[i]);
            }

            return sum;
        }

        private static double ComputeSumElementValue(List<double> variables, double coefficient, int[] combination)
        {
            double result = 1.0;

            for (int i = 0; i < combination.Length; i++)
            {
                result *= variables[combination[i] - 1];
            }

            return result * coefficient;
        }
    }
}
