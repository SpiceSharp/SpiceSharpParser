using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionParserFactory : IExpressionParserFactory
    {
        private readonly SpiceNetlistCaseSensitivitySettings _caseSensitivitySettings;

        public ExpressionParserFactory(SpiceNetlistCaseSensitivitySettings caseSensitivitySettings)
        {
            _caseSensitivitySettings = caseSensitivitySettings;
        }

        public ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true)
        {
            var parser = new Parser();
            var realBuilder = new CustomRealBuilder(context, parser, _caseSensitivitySettings, throwOnErrors, new VariablesFactory());
            var expressionParser = new ExpressionParser(parser, realBuilder, throwOnErrors);
            return expressionParser;
        }
    }
}