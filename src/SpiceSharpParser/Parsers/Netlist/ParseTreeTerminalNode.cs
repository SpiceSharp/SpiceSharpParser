using SpiceSharpParser.Lexers.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Netlist
{
    /// <summary>
    /// Terminal node in parse tree. It contains a token.
    /// </summary>
    public class ParseTreeTerminalNode : ParseTreeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeTerminalNode"/> class.
        /// </summary>
        /// <param name="token">A token for the terminal node.</param>
        /// <param name="parent">A parent of the terminal node.</param>
        public ParseTreeTerminalNode(SpiceToken token, ParseTreeNonTerminalNode parent)
            : base(parent)
        {
            if (parent == null)
            {
                throw new System.ArgumentNullException(nameof(parent));
            }

            Token = token ?? throw new System.ArgumentNullException(nameof(token));
        }

        /// <summary>
        /// Gets the token for the node.
        /// </summary>
        public SpiceToken Token { get; }

        /// <summary>
        /// Returns a string representation of the parse tree node.
        /// </summary>
        /// <returns>
        /// A string representation of the terminal node.
        /// </returns>
        public override string ToString()
        {
            return "Terminal: [" + ((Token.Lexem == "\r\n" || Token.Lexem == "\n") ? "newline" : Token.Lexem) + "]";
        }
    }
}