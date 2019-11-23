namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Enumeration that specifies whether to use a rule or skip to the next rule.
    /// </summary>
    public enum LexerRuleUseDecision
    {
        /// <summary>
        /// Specifies that the rule is used.
        /// </summary>
        Use = 0,

        /// <summary>
        /// Specifies that the next rule will be tried.
        /// </summary>
        Next = 1,
    }
}