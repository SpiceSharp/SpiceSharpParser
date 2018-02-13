namespace SpiceParser
{
    /// <summary>
    /// A tree node in parse tree
    /// </summary>
    public abstract class ParseTreeNode
    {
        public ParseTreeNode()
        {
        }

        public ParseTreeNode(ParseTreeNode parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Gets the parent of the tree node
        /// </summary>
        public ParseTreeNode Parent { get; }
    }
}
