namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public interface ISpiceTokenProvider
    {
        SpiceToken[] GetTokens(string netlist);
    }
}