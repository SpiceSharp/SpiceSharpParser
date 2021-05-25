namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionValueProvider : IExpressionValueProvider
    {
        private readonly IExpressionParserFactory _parserFactory;

        public ExpressionValueProvider(IExpressionParserFactory parserFactory)
        {
            _parserFactory = parserFactory;
        }

        public double GetExpressionValue(string expression, EvaluationContext context, bool @throw = true)
        {
            try
            {
                var parser = _parserFactory.Create(context, @throw);
                return parser.Parse(expression);
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
