using SpiceSharpParser.Connector.Evaluation;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters
{
    public class ExpressionExport : Export
    {

        public ExpressionExport(string expressionName, string expression, IEvaluator evaluator)
        {
            ExpressionName = expressionName;
            Evaluator = evaluator;
            Expression = expression;
        }

        public string ExpressionName { get; }

        public IEvaluator Evaluator { get; }

        public string Expression { get; }

        public override string TypeName => Expression;

        public override string Name => ExpressionName;

        public override string QuantityUnit => string.Empty;

        public override double Extract()
        {
            return Evaluator.EvaluateDouble(Expression);
        }
    }
}
