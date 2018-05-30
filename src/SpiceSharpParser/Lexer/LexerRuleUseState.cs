namespace SpiceSharpParser.Lexer
{
    /// <summary>
    /// Enumeration that specifies whether to skip or use a lexer token rule.
    /// </summary>
    public enum LexerRuleUseState
    {
        /// <summary>
        /// Use rule
        /// </summary>
        Use = 0,

        /// <summary>
        /// Skip rule
        /// </summary>
        Skip = 1,
    }
}
