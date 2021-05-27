namespace SpiceSharpParser.Lexers.Expressions
{
    /// <summary>
    /// The token types (also function as a state).
    /// </summary>
    /// <remarks>
    /// Code from SpiceSharpBehavioral.
    /// </remarks>
    public enum TokenType
    {
        /// <summary>
        /// A node identifier.
        /// </summary>
        Node,

        /// <summary>
        /// An identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// A number.
        /// </summary>
        Number,

        /// <summary>
        /// A comma.
        /// </summary>
        Comma,

        /// <summary>
        /// A plus sign.
        /// </summary>
        Plus,

        /// <summary>
        /// A minus sign.
        /// </summary>
        Minus,

        /// <summary>
        /// An asterisk (multiplication).
        /// </summary>
        Times,

        /// <summary>
        /// A forward slash (division).
        /// </summary>
        Divide,

        /// <summary>
        /// A percent (modulo).
        /// </summary>
        Mod,

        /// <summary>
        /// A power sign.
        /// </summary>
        Power,

        /// <summary>
        /// An equality (==).
        /// </summary>
        Equals,

        /// <summary>
        /// An inequality (!=)
        /// </summary>
        NotEquals,

        /// <summary>
        /// Less than.
        /// </summary>
        LessThan,

        /// <summary>
        /// Greater than.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Less or equal than.
        /// </summary>
        LessEqual,

        /// <summary>
        /// Greater or equal than.
        /// </summary>
        GreaterEqual,

        /// <summary>
        /// Or.
        /// </summary>
        Or,

        /// <summary>
        /// And.
        /// </summary>
        And,

        /// <summary>
        /// A bang (exclamation mark).
        /// </summary>
        Bang,

        /// <summary>
        /// A question mark.
        /// </summary>
        Huh,

        /// <summary>
        /// A colon.
        /// </summary>
        Colon,

        /// <summary>
        /// The at-sign.
        /// </summary>
        At,

        /// <summary>
        /// An assignment (=).
        /// </summary>
        Assign,

        /// <summary>
        /// A dot.
        /// </summary>
        Dot,

        /// <summary>
        /// The left parenthesis.
        /// </summary>
        LeftParenthesis,

        /// <summary>
        /// The right parenthesis.
        /// </summary>
        RightParenthesis,

        /// <summary>
        /// The left square bracket.
        /// </summary>
        LeftIndex,

        /// <summary>
        /// The right square bracket.
        /// </summary>
        RightIndex,

        /// <summary>
        /// An unknown character.
        /// </summary>
        Unknown,

        /// <summary>
        /// The end of the expression.
        /// </summary>
        EndOfExpression,
    }
}
