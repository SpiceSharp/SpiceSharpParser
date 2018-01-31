using System.Collections.Generic;

namespace SpiceParser
{
    public class ParseTreeNode
    {
        public ParseTreeNode Parent { get; set; }

        public ParseTreeNode()
        {
        }

        public virtual void Accept(ParseTreeVisitor visitor)
        {

        }
    }
}
