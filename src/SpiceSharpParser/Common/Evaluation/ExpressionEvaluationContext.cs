namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionEvaluationContext
    {
        public ExpressionEvaluationContext()
        {
        }

        public ExpressionContext ExpressionContext { get; set; }

        public IEvaluator Evaluator { get; set; }
    }
}
