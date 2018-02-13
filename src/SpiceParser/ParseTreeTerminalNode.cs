using NLexer;

namespace SpiceParser
{
    public class ParseTreeTerminalNode : ParseTreeNode
    {
        public ParseTreeTerminalNode(Token token, ParseTreeNonTerminalNode parent)
            : base(parent)
        {
            this.Token = token;
        }

        public Token Token { get; }

        public override string ToString()
        {
            return "Terminal: [" + ((Token.Lexem == "\r\n" || Token.Lexem == "\n") ? "newline" : Token.Lexem) + "]";
        }

        public override void Accept(ParseTreeVisitor visitor)
        {
            visitor.VisitParseTreeTerminal(this);
        }
    }
}
