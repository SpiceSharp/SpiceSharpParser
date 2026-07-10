using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Parsers.Expression;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionResolverFactory : IExpressionResolverFactory
    {
        private readonly SpiceNetlistCaseSensitivitySettings _caseSensitivitySettings;
        private readonly CompatibilityOptions _compatibility;

        public ExpressionResolverFactory(
            SpiceNetlistCaseSensitivitySettings caseSensitivitySettings,
            CompatibilityOptions compatibility = null)
        {
            _caseSensitivitySettings = caseSensitivitySettings;
            _compatibility = compatibility ?? CompatibilityOptions.None;
        }

        public ExpressionResolver Create(EvaluationContext context, bool throwOnErrors = true)
        {
            var variableFactory = new VariablesFactory();
            var realBuilder = new CustomRealBuilder(
                context,
                _caseSensitivitySettings,
                throwOnErrors,
                variableFactory,
                _compatibility);
            var resolver = new ExpressionResolver(
                realBuilder,
                context,
                throwOnErrors,
                _caseSensitivitySettings,
                variableFactory,
                _compatibility);
            return resolver;
        }
    }
}
