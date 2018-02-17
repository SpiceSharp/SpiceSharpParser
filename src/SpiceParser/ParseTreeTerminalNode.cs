using SpiceLexer;

namespace SpiceParser
{
    /// <summary>
    /// Terminal node in parse tree. It contains a token
    /// </summary>
    public class ParseTreeTerminalNode : ParseTreeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseTreeTerminalNode"/> class.
        /// </summary>
        public ParseTreeTerminalNode(SpiceToken token, ParseTreeNonTerminalNode parent)
            : base(parent)
        {
            this.Token = token;
        }

        /// <summary>
        /// Gets the token for the node
        /// </summary>
        public SpiceToken Token { get; }

        /// <summary>
        /// Returns a string representation of the parse tree node
        /// </summary>
        public override string ToString()
        {
            return "Terminal: [" + ((Token.Lexem == "\r\n" || Token.Lexem == "\n") ? "newline" : Token.Lexem) + "]";
        }
    }
}
