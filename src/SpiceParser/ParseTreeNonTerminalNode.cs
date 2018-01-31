using System.Collections.Generic;

namespace SpiceParser
{
    public class ParseTreeNonTerminalNode : ParseTreeNode
    {
        public string Name { get; set; }

        public List<ParseTreeNode> Children { get; private set; }

        public ParseTreeNonTerminalNode()
        {
            Children = new List<ParseTreeNode>();
        }

        public override void Accept(ParseTreeVisitor visitor)
        {
            foreach (var node in Children)
            {
                node.Accept(visitor);
            }
            visitor.VisitParseTreeNonTerminal(this);
        }
    }
}
