using SpiceNetlist;

namespace SpiceParser.Evaluation
{
    /// <summary>
    /// The non-terminal tree node evaluation value
    /// </summary>
    public class NonTerminalEvaluationValue : EvaluationValue
    {
        /// <summary>
        /// Gets or sets the value of non-terminal node
        /// </summary>
        public SpiceObject SpiceObject { get; set; }
    }
}
