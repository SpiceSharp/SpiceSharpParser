namespace NLexer
{
    /// <summary>
    /// Internal rule of lexer. It is use for creating common regular expression to use with LexerTokeRules
    /// </summary>
    public class LexerInternalRule : LexerRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerInternalRule"/> class.
        /// </summary>
        /// <param name="ruleName">Name of the rule</param>
        /// <param name="regularExpression">Regular expression</param>
        /// <param name="ignoreCase">Ignore case</param>
        public LexerInternalRule(string ruleName, string regularExpression, bool ignoreCase)
            : base(ruleName, regularExpression, ignoreCase)
        {
        }

        /// <summary>
        /// Clones the current instance
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="LexerInternalRule"/>
        /// </returns>
        public override LexerRule Clone()
        {
            return new LexerInternalRule(this.Name, this.RegularExpressionPattern, this.IgnoreCase);
        }
    }
}
