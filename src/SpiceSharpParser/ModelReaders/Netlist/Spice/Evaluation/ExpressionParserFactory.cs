using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionParserFactory : IExpressionParserFactory
    {
        private readonly ISpiceNetlistCaseSensitivitySettings _caseSensitivitySettings;

        public ExpressionParserFactory(ISpiceNetlistCaseSensitivitySettings caseSensitivitySettings)
        {
            _caseSensitivitySettings = caseSensitivitySettings;
        }

        public ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true)
        {
            var parser = new Parser();
            var realBuilder = new CustomRealBuilder(context, parser, _caseSensitivitySettings, throwOnErrors, new VariablesFactory());
            var variableFactory = new VariablesFactory();
            var expressionParser = new ExpressionParser(realBuilder, context, throwOnErrors, _caseSensitivitySettings, variableFactory);
            return expressionParser;
        }
    }
}