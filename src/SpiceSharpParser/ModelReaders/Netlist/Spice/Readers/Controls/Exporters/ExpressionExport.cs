using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters
{
    /// <summary>
    /// Describes a quantity that can be exported using data from expression.
    /// </summary>
    public class ExpressionExport : Export
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionExport"/> class.
        /// </summary>
        /// <param name="name">Name of export.</param>
        /// <param name="expressionName">Name of expression.</param>
        /// <param name="context"></param>
        public ExpressionExport(string name, string expressionName, EvaluationContext context)
            : base(context.Simulation)
        {
            Name = name;
            ExpressionName = expressionName;
            Context = context;
            Name = ExpressionName;
        }

        /// <summary>
        /// Gets the expression name.
        /// </summary>
        public string ExpressionName { get; }

        public EvaluationContext Context { get; }

        /// <summary>
        /// Gets the export unit.
        /// </summary>
        public override string QuantityUnit => string.Empty;

        /// <summary>
        /// Extract the quantity from simulated data.
        /// </summary>
        /// <returns>
        /// A quantity.
        /// </returns>
        public override double Extract()
        {
            return Context.Evaluator.EvaluateDouble(Context.GetExpression(ExpressionName));
        }
    }
}