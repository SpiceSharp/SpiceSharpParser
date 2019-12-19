using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation;

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
            bool present = false;
            var parser = _factory.Create(context);
            parser.SpicePropertyFound += (sender, e) =>
            {
                present = true;
                e.Apply(() => 0);
            };

            parser.Parse(expression);
            return present;
        }

        public bool HaveFunctions(string expression, EvaluationContext context)
        {
            bool present = false;
            var parser = _factory.Create(context);
            parser.FunctionFound += (sender, e) =>
            {
                present = true;
            };

            parser.Parse(expression);
            return present;
        }

        public IEnumerable<string> GetParameters(string expression, EvaluationContext context, bool @throw = true)
        {
            var parser = _factory.Create(context, @throw);
            var parameters = new List<string>();
            parser.VariableFound += (sender, e) =>
            {
                if (!parameters.Contains(e.Name))
                {
                    parameters.Add(e.Name);
                }
            };
            parser.SpicePropertyFound += (sender, e) => { e.Apply(() => 0); };

            parser.Parse(expression);
            return parameters;
        }
    }
}
