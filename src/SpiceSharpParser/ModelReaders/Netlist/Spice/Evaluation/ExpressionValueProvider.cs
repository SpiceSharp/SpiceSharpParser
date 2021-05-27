namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionValueProvider : IExpressionValueProvider
    {
        private readonly IExpressionParserFactory _parserFactory;

        public ExpressionValueProvider(IExpressionParserFactory parserFactory)
        {
            _parserFactory = parserFactory;
        }

        public double GetExpressionValue(string expression, object context, bool @throw = true)
        {
            try
            {
                var parser = _parserFactory.Create((EvaluationContext)context, @throw);
                return parser.Evaluate(expression);
            }
            catch
            {
                if (@throw)
                {
                    throw;
                }
                else
                {
                    return double.NaN;
                }
            }
        }
    }
}
