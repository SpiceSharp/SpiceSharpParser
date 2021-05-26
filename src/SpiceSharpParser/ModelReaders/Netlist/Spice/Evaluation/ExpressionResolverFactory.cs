using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionResolverFactory : IExpressionResolverFactory
    {
        private readonly SpiceNetlistCaseSensitivitySettings _caseSensitivitySettings;

        public ExpressionResolverFactory(SpiceNetlistCaseSensitivitySettings caseSensitivitySettings)
        {
            _caseSensitivitySettings = caseSensitivitySettings;
        }

        public ExpressionResolver Create(EvaluationContext context, bool throwOnErrors = true)
        {
            var parser = new Parser();
            var variableFactory = new VariablesFactory();
            var realBuilder = new CustomRealBuilder(context, parser, _caseSensitivitySettings, throwOnErrors, variableFactory);
            var resolver = new ExpressionResolver(parser, realBuilder, context, throwOnErrors, _caseSensitivitySettings, variableFactory);
            return resolver;
        }
    }
}