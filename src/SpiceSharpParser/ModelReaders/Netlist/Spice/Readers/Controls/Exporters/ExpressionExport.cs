using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
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
        /// <param name="name">Name of export</param>
        /// <param name="expressionName">Name of expression</param>
        /// <param name="expression">Expression</param>
        /// <param name="evaluator">Evaluator</param>
        public ExpressionExport(string name, string expressionName, string expression, IEvaluator evaluator, Simulation simulation)
            : base(simulation)
        {
            Name = name;
            ExpressionName = expressionName;
            Evaluator = evaluator;
            Expression = expression;
            Name = ExpressionName;

        }

        /// <summary>
        /// Gets the expression name.
        /// </summary>
        public string ExpressionName { get; }

        /// <summary>
        /// Gets or sets the evalutor.
        /// </summary>
        public IEvaluator Evaluator { get; set; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public override string TypeName => Expression;

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
            return Evaluator.EvaluateDouble(Expression);
        }
    }
}
