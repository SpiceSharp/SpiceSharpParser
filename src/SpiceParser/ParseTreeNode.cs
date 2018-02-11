namespace SpiceParser
{
    public class ParseTreeNode
    {
        public ParseTreeNode()
        {
        }

        public ParseTreeNode(ParseTreeNode parent)
        {
            Parent = parent;
        }

        public ParseTreeNode Parent { get; }

        public virtual void Accept(ParseTreeVisitor visitor)
        {
        }
    }
}
