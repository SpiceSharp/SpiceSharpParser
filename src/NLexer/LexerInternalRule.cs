namespace NLexer
{
    /// <summary>
    /// Internal rule of lexer. It is use for creating common regular expression to use with LexerTokeRules
    /// </summary>
    public class LexerInternalRule : LexerRule
    {
        public LexerInternalRule(string name, string regularExpression)
            : base(name, regularExpression)
        {
        }

        internal override LexerRule Clone()
        {
            return new LexerInternalRule(this.Name, this.RegularExpressionPattern);
        }
    }
}
