using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public class ExpressionFactory
    {
        public static string CreateTableExpression(string tableParameter, ExpressionEqualParameter eep)
        {
            var expression = $"table({tableParameter},{string.Join(",", eep.Points.Values.Select(v => string.Join(", ", v.Values.Select(a => a.Image).ToArray())).ToArray())})";
            return expression;
        }

        public static string CreatePolyVoltageExpression(int dimension, ParameterCollection polyArguments)
        {
            bool pointFormat = polyArguments.Any(p => p is PointParameter);

            if (pointFormat)
            {
                var variables = polyArguments.Take(dimension);
                var variablesString = string.Join(",", variables.Select(v => $"v({((PointParameter)v).Values.Items[0].Image},{((PointParameter)v).Values.Items[1].Image})"));

                var coefficients = polyArguments.Skip(dimension);
                return CreatePolyExpression(dimension, coefficients, variablesString);
            }
            else
            {
                var variables = polyArguments.Take(2 * dimension);
                var voltages = new List<string>();

                for (var i = 0; i < dimension; i++)
                {
                    string voltage = $"v({variables[2 * i].Image},{variables[(2 * i) + 1].Image})";
                    voltages.Add(voltage);
                }

                var variablesString = string.Join(",", voltages.ToArray());
                var coefficients = polyArguments.Skip(2 * dimension);

                return CreatePolyExpression(dimension, coefficients, variablesString);
            }
        }

        public static string CreatePolyCurrentExpression(int dimension, ParameterCollection polyArguments)
        {
            var variables = polyArguments.Take(dimension);
            var voltages = new List<string>();
            for (var i = 0; i < dimension; i++)
            {
                string voltage = $"i({variables[i].Image})";
                voltages.Add(voltage);
            }

            var variablesString = string.Join(",", voltages.ToArray());
            var coefficients = polyArguments.Skip(dimension);

            return CreatePolyExpression(dimension, coefficients, variablesString);
        }

        private static string CreatePolyExpression(
            int dimension,
            ParameterCollection coefficients,
            string variablesString)
        {
            var coefficientsString = string.Join(",", coefficients.Select(c => c.Image).ToArray());
            var expression = $"poly({dimension}, {variablesString}, {coefficientsString})";
            return expression;
        }
    }
}