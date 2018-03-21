using SpiceNetlist;

namespace SpiceParser.Translation
{
    /// <summary>
    /// The non-terminal tree node evaluation value
    /// </summary>
    public class ParseTreeNonTerminalEvaluationValue : ParseTreeNodeEvaluationValue
    {
        /// <summary>
        /// Gets or sets the value of non-terminal node
        /// </summary>
        public SpiceObject SpiceObject { get; set; }
    }
}
