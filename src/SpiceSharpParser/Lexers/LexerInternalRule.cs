namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Internal rule of lexer. It is use for creating common regular expression to use with LexerTokeRules.
    /// </summary>
    public class LexerInternalRule : LexerRegexRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerInternalRule"/> class.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        /// <param name="regularExpression">Regular expression.</param>
        public LexerInternalRule(string ruleName, string regularExpression)
            : base(ruleName, regularExpression, false)
        {
        }

        /// <summary>
        /// Clones the current instance.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="LexerInternalRule"/>.
        /// </returns>
        public override LexerRegexRule Clone()
        {
            return new LexerInternalRule(Name, RegularExpressionPattern);
        }
    }
}