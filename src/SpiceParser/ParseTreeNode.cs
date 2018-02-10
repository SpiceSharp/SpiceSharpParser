namespace SpiceParser
{
    public class ParseTreeNode
    {
        public ParseTreeNode Parent { get; }

        public ParseTreeNode()
        {
        }

        public ParseTreeNode(ParseTreeNode parent)
        {
            Parent = parent;
        }

        public virtual void Accept(ParseTreeVisitor visitor)
        {

        }
    }
}
