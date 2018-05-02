using SpiceSharpParser.Connector.Evaluation;

namespace SpiceSharpParser.Connector.Processors.Controls.Exporters
{
    /// <summary>
    /// Describes a quantity that can be exported using data from expression.
    /// </summary>
    public class ExpressionExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionExport"/> class.
        /// </summary>
        /// <param name="expressionName">Name of expression</param>
        /// <param name="expression">Expression</param>
        /// <param name="evaluator">Evaluator</param>
        public ExpressionExport(string expressionName, string expression, IEvaluator evaluator)
        {
            ExpressionName = expressionName;
            Evaluator = evaluator;
            Expression = expression;
            Name = ExpressionName;
        }

        /// <summary>
        /// Gets the expression name
        /// </summary>
        public string ExpressionName { get; }

        /// <summary>
        /// Gets the evalutor
        /// </summary>
        public IEvaluator Evaluator { get; }

        /// <summary>
        /// Gets the expression
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets the type name
        /// </summary>
        public override string TypeName => Expression;

        /// <summary>
        /// Gets the export unit
        /// </summary>
        public override string QuantityUnit => string.Empty;

        /// <summary>
        /// Extract the quantity from simulated data
        /// </summary>
        /// <returns>
        /// A quantity
        /// </returns>
        public override double Extract()
        {
            return Evaluator.EvaluateDouble(Expression);
        }
    }
}
