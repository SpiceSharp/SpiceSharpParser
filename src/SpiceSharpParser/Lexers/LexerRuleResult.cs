namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Enumeration that specifies whether a matched lexer token rule should return or ignore token.
    /// </summary>
    public enum LexerRuleResult
    {
        /// <summary>
        /// Specifies that the token is retured if it's a best match.
        /// </summary>
        ReturnToken = 0,

        /// <summary>
        /// Specifies that the token is ignored.
        /// </summary>
        IgnoreToken = 1,
    }
}
