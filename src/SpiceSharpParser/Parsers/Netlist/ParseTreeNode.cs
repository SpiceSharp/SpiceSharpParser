namespace SpiceSharpParser.Parsers.Netlist
{
    /// <summary>
    /// A tree node in parse tree.
    /// </summary>
    public abstract class ParseTreeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class without the parent.
        /// </summary>
        protected ParseTreeNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class.
        /// </summary>
        /// <param name="parent">A parent of the tree node.</param>
        protected ParseTreeNode(ParseTreeNode parent)
        {
            Parent = parent ?? throw new System.ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// Gets the parent of the tree node.
        /// </summary>
        public ParseTreeNode Parent { get; }
    }
}