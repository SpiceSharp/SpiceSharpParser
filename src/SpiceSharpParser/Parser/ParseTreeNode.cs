namespace SpiceSharpParser.Parser
{
    /// <summary>
    /// A tree node in parse tree
    /// </summary>
    public abstract class ParseTreeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class without the parent.
        /// </summary>
        public ParseTreeNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNode"/> class.
        /// </summary>
        /// <param name="parent">A parent of the tree node</param>
        public ParseTreeNode(ParseTreeNode parent)
        {
            Parent = parent ?? throw new System.ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// Gets the parent of the tree node
        /// </summary>
        public ParseTreeNode Parent { get; }
    }
}
