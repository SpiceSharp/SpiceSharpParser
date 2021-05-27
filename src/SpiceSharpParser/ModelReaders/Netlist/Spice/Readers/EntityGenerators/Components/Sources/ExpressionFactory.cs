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
            var expression = $"table({tableParameter},{string.Join(",", points.Select(v => string.Join(", ", v.Values.Select(a => a.Value).ToArray())).ToArray())})";
            return expression;
        }

        public static string CreatePolyVoltageExpression(int dimension, ParameterCollection polyArguments, IEvaluationContext evaluationContext)
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
                        $"v({((PointParameter)v).Values.Items[0].Value},{((PointParameter)v).Values.Items[1].Value})")
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
                    string voltage = $"v({variables[2 * i].Value},{variables[(2 * i) + 1].Value})";
                    voltages.Add(voltage);
                }

                ParameterCollection coefficients = polyArguments.Skip(2 * dimension);

                return CreatePolyExpression(dimension, coefficients, voltages, evaluationContext);
            }
        }

        public static string CreatePolyCurrentExpression(int dimension, ParameterCollection polyArguments, IEvaluationContext context)
        {
            var variables = polyArguments.Take(dimension);
            var voltages = new List<string>();
            for (var i = 0; i < dimension; i++)
            {
                string voltage = $"i({variables[i].Value})";
                voltages.Add(voltage);
            }

            ParameterCollection coefficients = polyArguments.Skip(dimension);

            return CreatePolyExpression(dimension, coefficients, voltages, context);
        }

        private static string CreatePolyExpression(
            int dimension,
            ParameterCollection coefficients,
            List<string> variables,
            IEvaluationContext context)
        {
            if (coefficients.Count == 1 && coefficients[0] is PointParameter pp)
            {
                var result = PolyFunction.GetExpression(
                    dimension,
                    pp.Values.Items.Select(c => context.Evaluator.EvaluateDouble(c.Value)).ToList(),
                    variables);
                return result;
            }
            else
            {
                var result = PolyFunction.GetExpression(
                    dimension,
                    coefficients.Select(c => context.Evaluator.EvaluateDouble(c.Value)).ToList(),
                    variables);

                return result;
            }
        }
    }
}