using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    public class ExpressionFactory
    {
        public static string CreateTableExpression(string tableParameter, IEnumerable<PointParameter> points)
        {
            var expression = $"table({tableParameter},{string.Join(",", points.Select(v => string.Join(", ", v.Values.Select(a => a.Image).ToArray())).ToArray())})";
            return expression;
        }

        public static string CreatePolyVoltageExpression(int dimension, ParameterCollection polyArguments)
        {
            if (polyArguments.Count == 0)
            {
                throw new WrongParametersCountException("Wrong parameter count for poly expression");
            }

            bool voltagesAreSpecifiedAsPoints = polyArguments[0] is PointParameter;

            if (voltagesAreSpecifiedAsPoints)
            {
                var variables = polyArguments.Take(dimension);

                if (variables.Count < dimension)
                {
                    throw new WrongParametersCountException("Wrong parameter count for poly expression");
                }

                if (variables.Any(v => !(v is PointParameter)))
                {
                    throw new WrongParameterTypeException("Wrong parameter type for poly expression");
                }

                var variablesString = string.Join(",", variables.Select(v => $"v({((PointParameter)v).Values.Items[0].Image},{((PointParameter)v).Values.Items[1].Image})"));

                var coefficients = polyArguments.Skip(dimension);
                return CreatePolyExpression(dimension, coefficients, variablesString);
            }
            else
            {
                var variables = polyArguments.Take(2 * dimension);

                if (variables.Count < 2 * dimension)
                {
                    throw new WrongParametersCountException("Wrong parameter count for poly expression");
                }

                if (variables.Any(v => !(v is SingleParameter)))
                {
                    throw new WrongParameterTypeException("Wrong parameter type for poly expression");
                }

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
            if (coefficients.Count == 1 && coefficients[0] is PointParameter pp)
            {
                var coefficientsString = string.Join(",", pp.Values.Select(c => c.Image).ToArray());
                var expression = $"poly({dimension}, {variablesString}, {coefficientsString})";
                return expression;
            }
            else
            {
                var coefficientsString = string.Join(",", coefficients.Select(c => c.Image).ToArray());
                var expression = $"poly({dimension}, {variablesString}, {coefficientsString})";
                return expression;
            }
        }
    }
}