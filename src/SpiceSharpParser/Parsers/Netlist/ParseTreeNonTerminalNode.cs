using System;
using System.Collections.Generic;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.Parsers.Netlist
{
    /// <summary>
    /// A non-terminal node in a parse tree.
    /// </summary>
    public class ParseTreeNonTerminalNode : ParseTreeNode, ILocationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNonTerminalNode"/> class.
        /// </summary>
        /// <param name="name">A name of the non-terminal node.</param>
        public ParseTreeNonTerminalNode(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Children = new List<ParseTreeNode>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeNonTerminalNode"/> class.
        /// </summary>
        /// <param name="parent">A parent of the node.</param>
        /// <param name="name">A name of the non-terminal node.</param>
        public ParseTreeNonTerminalNode(ParseTreeNode parent, string name)
            : base(parent)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Children = new List<ParseTreeNode>();
        }

        /// <summary>
        /// Gets name of non-terminal.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets children of non-terminal.
        /// </summary>
        public List<ParseTreeNode> Children { get; }

        /// <summary>
        /// Gets or sets start column index.
        /// </summary>
        public int StartColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets start column index.
        /// </summary>
        public int EndColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets line number.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or set file name.
        /// </summary>
        public string FileName { get; set; }
    }
}