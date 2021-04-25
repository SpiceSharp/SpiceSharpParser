namespace SpiceSharpParser.Lexers.BusPrefix
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
    }
}
