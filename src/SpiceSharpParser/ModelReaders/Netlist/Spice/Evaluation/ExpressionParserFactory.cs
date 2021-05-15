using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionParserFactory : IExpressionParserFactory
    {
        private readonly ISpiceNetlistCaseSensitivitySettings caseSensitivitySettings;

        public ExpressionParserFactory(ISpiceNetlistCaseSensitivitySettings caseSensitivitySettings)
        {
            this.caseSensitivitySettings = caseSensitivitySettings;
        }

        public ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true)
        {
            var parser = new Parser();
            var realBuilder = new CustomRealBuilder(context, parser, caseSensitivitySettings, throwOnErrors, new VariablesFactory());
            var variableFactory = new VariablesFactory();
            var expressionParser = new ExpressionParser(realBuilder, context, throwOnErrors, caseSensitivitySettings, variableFactory);
            return expressionParser;
        }
    }
}