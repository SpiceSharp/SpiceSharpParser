namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionEvaluationContext
    {
        public ExpressionEvaluationContext()
        {
            ExpressionContext = new ExpressionContext();
        }

        public ExpressionContext ExpressionContext { get; set; }

        public IEvaluator Evaluator { get; set; }
    }
}
