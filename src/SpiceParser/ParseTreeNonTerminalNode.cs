using System.Collections.Generic;

namespace SpiceParser
{
    /// <summary>
    /// Non terminal node in parse tree
    /// </summary>
    public class ParseTreeNonTerminalNode : ParseTreeNode
    {
        public ParseTreeNonTerminalNode(ParseTreeNode parent, string name)
            : base(parent)
        {
            Children = new List<ParseTreeNode>();
            Name = name;
        }

        /// <summary>
        /// Gets name of non terminal
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets children of non terminal
        /// </summary>
        public List<ParseTreeNode> Children { get; }
    }
}
