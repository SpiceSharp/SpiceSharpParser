namespace SpiceSharpParser.Lexers.Netlist.Spice
{
    public interface ISpiceTokenProviderPool
    {
        ISpiceTokenProvider GetSpiceTokenProvider(SpiceLexerSettings settings);
    }
}