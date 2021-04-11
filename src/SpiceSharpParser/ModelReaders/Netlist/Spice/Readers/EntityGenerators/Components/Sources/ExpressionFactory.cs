using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math;
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

        public static string CreatePolyVoltageExpression(int dimension, ParameterCollection polyArguments, EvaluationContext evaluationContext)
        {
            if (polyArguments.Count == 0)
            {
                throw new SpiceSharpParserException("Wrong parameter count for poly expression", polyArguments.LineInfo);
            }

            bool voltagesAreSpecifiedAsPoints = polyArguments[0] is PointParameter;

            if (voltagesAreSpecifiedAsPoints)
            {
                var variables = polyArguments.Take(dimension);

                if (variables.Count < dimension)
                {
                    throw new SpiceSharpParserException("Wrong parameter count for poly expression", variables.LineInfo);
                }

                if (variables.Any(v => !(v is PointParameter)))
                {
                    throw new SpiceSharpParserException("Wrong parameter type for poly expression", variables.LineInfo);
                }

                var variablesList = variables.Select(v =>
                        $"v({((PointParameter)v).Values.Items[0].Image},{((PointParameter)v).Values.Items[1].Image})")
                    .ToList();

                var coefficients = polyArguments.Skip(dimension);
                return CreatePolyExpression(dimension, coefficients, variablesList, evaluationContext);
            }
            else
            {
                var variables = polyArguments.Take(2 * dimension);

                if (variables.Count < 2 * dimension)
                {
                    throw new SpiceSharpParserException("Wrong parameter count for poly expression", variables.LineInfo);
                }

                if (variables.Any(v => !(v is SingleParameter)))
                {
                    throw new SpiceSharpParserException("Wrong parameter type for poly expression", variables.LineInfo);
                }

                var voltages = new List<string>();
                for (var i = 0; i < dimension; i++)
                {
                    string voltage = $"v({variables[2 * i].Image},{variables[(2 * i) + 1].Image})";
                    voltages.Add(voltage);
                }

                ParameterCollection coefficients = polyArguments.Skip(2 * dimension);

                return CreatePolyExpression(dimension, coefficients, voltages, evaluationContext);
            }
        }

        public static string CreatePolyCurrentExpression(int dimension, ParameterCollection polyArguments, EvaluationContext context)
        {
            var variables = polyArguments.Take(dimension);
            var voltages = new List<string>();
            for (var i = 0; i < dimension; i++)
            {
                string voltage = $"i({variables[i].Image})";
                voltages.Add(voltage);
            }

            ParameterCollection coefficients = polyArguments.Skip(dimension);

            return CreatePolyExpression(dimension, coefficients, voltages, context);
        }

        private static string CreatePolyExpression(
            int dimension,
            ParameterCollection coefficients,
            List<string> variables,
            EvaluationContext context)
        {
            if (coefficients.Count == 1 && coefficients[0] is PointParameter pp)
            {
                var result = PolyFunction.GetExpression(
                    dimension,
                    pp.Values.Items.Select(c => context.Evaluate(c.Image)).ToList(),
                    variables);
                return result;
            }
            else
            {
                var result = PolyFunction.GetExpression(
                    dimension,
                    coefficients.Select(c => context.Evaluate(c.Image)).ToList(),
                    variables);

                return result;
            }
        }
    }
}