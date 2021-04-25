using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionFeaturesReader : IExpressionFeaturesReader
    {
        private readonly IExpressionParserFactory _factory;

        public ExpressionFeaturesReader(IExpressionParserFactory factory)
        {
            _factory = factory;
        }

        public bool HaveSpiceProperties(string expression, EvaluationContext context)
        {
            var parser = _factory.Create(context, false);
            var variables = parser.GetVariables(expression);

            var voltageExportFactory = new VoltageExporter();
            var currentExportFactory = new CurrentExporter();

            foreach (var variable in variables)
            {
                var variableName = variable.ToLower();

                if (currentExportFactory.CreatedTypes.Any(type => variableName.StartsWith(type)))
                {
                    return true;
                }

                if (voltageExportFactory.CreatedTypes.Any(type => variableName.StartsWith(type)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HaveFunctions(string expression, EvaluationContext context)
        {
            var parser = _factory.Create(context, false);
            var functions = parser.GetFunctions(expression);
            return functions.Any();
        }

        public IEnumerable<string> GetParameters(string expression, EvaluationContext context, bool @throw = true)
        {
            var parser = _factory.Create(context, @throw);
            return parser.GetVariables(expression);
        }

        public bool HaveFunction(string expression, string functionName, EvaluationContext context)
        {
            var parser = _factory.Create(context, false);
            var functions = parser.GetFunctions(expression);
            return functions.Any(function => function.ToLower() == functionName.ToLower());
        }
    }
}
