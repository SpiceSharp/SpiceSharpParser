using SpiceSharp;
using System;

namespace SpiceSharpParser.Lexers.BusPrefix
{
    /// <summary>
    /// A lexer that will tokenize bus prefix expressions.
    /// </summary>
    public class Lexer
    {
        private readonly string _expression;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public Lexer(string expression)
        {
            _expression = expression;
            _index = -1;
        }

        /// <summary>
        /// Gets the current character.
        /// </summary>
        /// <value>
        /// The current character.
        /// </value>
        public char Current
        {
            get
            {
                return _index < _expression.Length ? _expression[_index] : '\0';
            }
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TokenType Token { get; private set; }

        public int Index { get => _index; set => _index = value; }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        public void ReadToken(bool skipSpaces = true)
        {
            _index++;

            // Skip spaces
            while (Current == ' ' && skipSpaces)
            {
                _index++;
            }

            // Nothing left to read!
            if (Current == '\0' || Current == '\r' || Current == '\n')
            {
                Token = TokenType.EndOfExpression;
                return;
            }

            // Initial classification of the current token
            Token = Current switch
            {
                '*' => TokenType.Times,
                ',' => TokenType.Comma,
                ' ' => TokenType.Space,
                '<' => TokenType.LessThan,
                '>' => TokenType.GreaterThan,
                '(' => TokenType.LeftParenthesis,
                ')' => TokenType.RightParenthesis,
                char mc when mc >= '0' && mc <= '9' => TokenType.Digit,
                char mc when (mc >= 'a' && mc <= 'z') || (mc >= 'A' && mc <= 'Z') || mc == '_' || mc == '$' => TokenType.Letter,
                _ => throw new Exception("Unrecognized character found: {0} at position {1}".FormatString(Current, _index)),
            };
        }

        /// <summary>
        /// Resets the lexer to the start of the input.
        /// </summary>
        public void Reset()
        {
            _index = 0;
        }
    }
}
