using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionParserFactory : IExpressionParserFactory
    {
        private readonly SpiceNetlistCaseSensitivitySettings _caseSensitivitySettings;
        private readonly CompatibilityOptions _compatibility;

        public ExpressionParserFactory(
            SpiceNetlistCaseSensitivitySettings caseSensitivitySettings,
            CompatibilityOptions compatibility = null)
        {
            _caseSensitivitySettings = caseSensitivitySettings;
            _compatibility = compatibility ?? CompatibilityOptions.None;
        }

        public ExpressionParser Create(EvaluationContext context, bool throwOnErrors = true)
        {
            var realBuilder = new CustomRealBuilder(
                context,
                _caseSensitivitySettings,
                throwOnErrors,
                new VariablesFactory(),
                _compatibility);
            var expressionParser = new ExpressionParser(realBuilder, throwOnErrors, _compatibility);
            return expressionParser;
        }
    }
}
