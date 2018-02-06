namespace SpiceParser
{
    public class ParseTreeNode
    {
        public ParseTreeNode Parent { get; private set; }

        public ParseTreeNode()
        {
        }

        public ParseTreeNode(ParseTreeNode parent)
        {
            this.Parent = parent;
        }

        public virtual void Accept(ParseTreeVisitor visitor)
        {

        }
    }
}
