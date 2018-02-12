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
        /// <param name="name"></param>
        /// <param name="regularExpression"></param>
        public LexerInternalRule(string name, string regularExpression)
            : base(name, regularExpression)
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
            return new LexerInternalRule(this.Name, this.RegularExpressionPattern);
        }
    }
}
