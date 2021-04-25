namespace SpiceSharpParser.Lexers.BusSuffix
{
    /// <summary>
    /// The token types.
    /// </summary>
    public enum TokenType
    {
        EndOfExpression,
        LessThan,
        GreaterThan,
        Times,
        Digit,
        Letter,
        Space,
        LeftParenthesis,
        RightParenthesis,
        Comma,
        Colon,
    }
}
