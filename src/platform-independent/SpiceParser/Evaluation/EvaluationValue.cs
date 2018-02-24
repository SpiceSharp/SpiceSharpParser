namespace SpiceParser.Evaluation
{
    /// <summary>
    /// The tree node evaluation value
    /// </summary>
    public abstract class EvaluationValue
    {
        /// <summary>
        /// Gets or sets reference to the parse tree node
        /// </summary>
        public ParseTreeNode Node { get; set; }
    }
}
