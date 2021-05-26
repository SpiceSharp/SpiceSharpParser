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
            var variableFactory = new VariablesFactory();
            var realBuilder = new CustomRealBuilder(context, _caseSensitivitySettings, throwOnErrors, variableFactory);
            var resolver = new ExpressionResolver(realBuilder, context, throwOnErrors, _caseSensitivitySettings, variableFactory);
            return resolver;
        }
    }
}