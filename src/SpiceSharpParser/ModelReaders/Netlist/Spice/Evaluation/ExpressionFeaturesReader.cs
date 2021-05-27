using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionFeaturesReader : IExpressionFeaturesReader
    {
        private readonly IExpressionParserFactory _expressionFactory;

        private readonly IExpressionResolverFactory _resolverFactory;

        public ExpressionFeaturesReader(IExpressionParserFactory expressionFactory, IExpressionResolverFactory resolverFactory)
        {
            _expressionFactory = expressionFactory;
            _resolverFactory = resolverFactory;
        }

        public bool HaveSpiceProperties(string expression, EvaluationContext context)
        {
            var resolver = _resolverFactory.Create(context, false);
            var node = resolver.Resolve(expression);

            var parser = _expressionFactory.Create(context, false);
            var variables = parser.GetVariables(node);

            var voltageExportFactory = new VoltageExporter();
            var currentExportFactory = new CurrentExporter();

            foreach (var variable in variables)
            {
                var variableName = variable.ToString().ToLower();

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
            var parser = _expressionFactory.Create(context, false);
            var functions = parser.GetFunctions(expression);
            return functions.Any();
        }

        public IEnumerable<string> GetParameters(string expression, EvaluationContext context, bool @throw = true)
        {
            var parser = _expressionFactory.Create(context, @throw);
            return parser.GetVariables(expression).Select(node => node.ToString());
        }

        public bool HaveFunction(string expression, string functionName, EvaluationContext context)
        {
            var parser = _expressionFactory.Create(context, false);
            var functions = parser.GetFunctions(expression);
            return functions.Any(function => function.ToLower() == functionName.ToLower());
        }
    }
}
