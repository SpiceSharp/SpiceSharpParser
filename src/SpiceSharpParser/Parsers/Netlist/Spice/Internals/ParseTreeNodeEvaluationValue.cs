namespace SpiceSharpParser.Parsers.Netlist.Spice.Internals
{
    /// <summary>
    /// The tree node evaluation value.
    /// </summary>
    public abstract class ParseTreeNodeEvaluationValue
    {
        /// <summary>
        /// Gets or sets reference to the parse tree node.
        /// </summary>
        public ParseTreeNode Node { get; set; }
    }
}