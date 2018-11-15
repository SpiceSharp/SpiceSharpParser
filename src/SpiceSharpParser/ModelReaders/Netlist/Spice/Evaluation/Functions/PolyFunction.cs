using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions
{
    public class PolyFunction
    {
        /// <summary>
        /// Creates a poly() function.
        /// </summary>
        /// <returns>
        /// A new instance of a poly() function.
        /// </returns>
        public static Function Create()
        {
            Function function = new Function();
            function.Name = "poly";
            function.VirtualParameters = false;
            function.ArgumentsCount = -1;

            function.DoubleArgsLogic = (image, args, evaluator, context) =>
                {
                    var dimension = (int)args[0];
                    List<double> variables = new List<double>();

                    if (args.Length < dimension + 1)
                    {
                        throw new Exception("To less variables for poly");
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
                };

            return function;
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
