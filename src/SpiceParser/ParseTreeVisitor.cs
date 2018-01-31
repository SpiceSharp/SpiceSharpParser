namespace SpiceParser
{
    public class ParseTreeVisitor
    {
        public virtual void VisitParseTreeTerminal(ParseTreeTerminalNode node)
        {
        }

        public virtual void VisitParseTreeNonTerminal(ParseTreeNonTerminalNode node)
        {
        }
    }
}
