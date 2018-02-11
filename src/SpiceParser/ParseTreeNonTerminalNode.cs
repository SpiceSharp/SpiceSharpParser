using System.Collections.Generic;

namespace SpiceParser
{
    public class ParseTreeNonTerminalNode : ParseTreeNode
    {
        public ParseTreeNonTerminalNode(ParseTreeNode parent, string name)
            : base(parent)
        {
            Children = new List<ParseTreeNode>();
            Name = name;
        }

        public string Name { get; set; }

        public List<ParseTreeNode> Children { get; set; }

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
