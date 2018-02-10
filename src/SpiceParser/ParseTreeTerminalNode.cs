using NLexer;

namespace SpiceParser
{
    public class ParseTreeTerminalNode : ParseTreeNode
    {
        public Token Token { get; }

        public ParseTreeTerminalNode(Token token, ParseTreeNonTerminalNode parent) : base(parent)
        {
            this.Token = token;
        }

        public override string ToString()
        {
            return "Terminal: [" + ((Token.Value == "\r\n" || Token.Value == "\n") ? "newline" : Token.Value) + "]";
        }

        public override void Accept(ParseTreeVisitor visitor)
        {
            visitor.VisitParseTreeTerminal(this);
        }
    }
}
