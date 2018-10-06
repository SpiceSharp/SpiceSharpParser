namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public interface ISpiceTokenProvider
    {
        SpiceToken[] GetTokens(string netlist, bool IsDotStatementCaseSensitive, bool hasTitle, bool isEndRequired);
    }
}
