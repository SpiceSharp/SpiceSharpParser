using System.Collections.Generic;

namespace SpiceParser
{
    /// <summary>
    /// Non terminal node in parse tree
    /// </summary>
    public class ParseTreeNonTerminalNode : ParseTreeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNonTerminalNode"/> class.
        /// </summary>
        /// <param name="parent">A parent of the node</param>
        /// <param name="name">A name of the non-terminal node</param>
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
