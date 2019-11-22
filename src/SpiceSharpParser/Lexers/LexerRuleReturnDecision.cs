namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Enumeration that specifies whether a matched lexer token rule should return or ignore token.
    /// </summary>
    public enum LexerRuleReturnDecision
    {
        /// <summary>
        /// Specifies that the token is returned if it's a best match.
        /// </summary>
        ReturnToken = 0,

        /// <summary>
        /// Specifies that the token is ignored.
        /// </summary>
        IgnoreToken = 1,
    }
}