using SpiceSharp;
using System;
using System.Text;

namespace SpiceSharpParser.Lexers.Expressions
{
    /// <summary>
    /// A lexer that will tokenize expressions.
    /// </summary>
    public class Lexer
    {
        private const int _initialWordSize = 16;
        private readonly StringBuilder _builder = new StringBuilder(_initialWordSize);
        private readonly string _expression;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public Lexer(string expression)
        {
            _expression = expression;
            _index = 0;
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

        /// <summary>
        /// Gets the last token.
        /// </summary>
        /// <value>
        /// The last token.
        /// </value>
        public TokenType LastToken { get; private set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content => _builder.ToString();

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        public int Index { get => _index; set => _index = value; }

        /// <summary>
        /// Gets the builder length.
        /// </summary>
        public int BuilderLength => _builder.Length;

        /// <summary>
        /// Reads the next token.
        /// </summary>
        public void ReadToken()
        {
            LastToken = Token;
            _builder.Clear();

            // Skip spaces
            while (Current == ' ')
            {
                _index++;
            }

            // Nothing left to read!
            if (Current == '\0')
            {
                Token = TokenType.EndOfExpression;
                return;
            }

            if (Current == '\r' || Current == '\n')
            {
                Token = TokenType.EndOfExpression;
                return;
            }

            // Initial classification of the current token
            _builder.Append(Current);
            Token = Current switch
            {
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '*' => TokenType.Times,
                '/' => TokenType.Divide,
                '%' => TokenType.Mod,
                '^' => TokenType.Power,
                '?' => TokenType.Huh,
                ':' => TokenType.Colon,
                '(' => TokenType.LeftParenthesis,
                ')' => TokenType.RightParenthesis,
                '[' => TokenType.LeftIndex,
                ']' => TokenType.RightIndex,
                '!' => TokenType.Bang,
                ',' => TokenType.Comma,
                '<' => TokenType.LessThan,
                '>' => TokenType.GreaterThan,
                '=' => TokenType.Assign,
                '@' => TokenType.At,
                '.' => TokenType.Dot,
                '&' => TokenType.And,
                '|' => TokenType.Or,
                char mc when mc >= '0' && mc <= '9' => TokenType.Number,
                char mc when (mc >= 'a' && mc <= 'z') || (mc >= 'A' && mc <= 'Z') || mc == '_' => TokenType.Identifier,
                _ => throw new Exception("Unrecognized character found: {0} at position {1}".FormatString(Current, _index)),
            };
            _index++; // Consume the character

            // For some cases, the follow-up character may change the type of token!
            var prevToken = Token;
            switch (Token)
            {
                case TokenType.Bang:
                    if (One(c => c == '='))
                    {
                        Token = TokenType.NotEquals;
                    }

                    break;
                case TokenType.LessThan:
                    if (One(c => c == '='))
                    {
                        Token = TokenType.LessEqual;
                    }

                    break;
                case TokenType.GreaterThan:
                    if (One(c => c == '='))
                    {
                        Token = TokenType.GreaterEqual;
                    }

                    break;
                case TokenType.Assign:
                    if (One(c => c == '='))
                    {
                        Token = TokenType.Equals;
                    }

                    break;
                case TokenType.And:
                    if (!One(c => c == '&'))
                    {
                        throw new Exception("Invalid AND operator at position {0}".FormatString(_index));
                    }

                    break;
                case TokenType.Or:
                    if (!One(c => c == '|'))
                    {
                        throw new Exception("Invalid OR operator at position {0}".FormatString(_index));
                    }

                    break;
                case TokenType.Number:
                    Any(char.IsDigit); // whole number part
                    if (Current == '.')
                    {
                        Consume();
                        Any(char.IsDigit); // Fraction
                    }

                    if (One(c => c == 'e' || c == 'E'))
                    {
                        // Exponential notation (possibly)
                        // If a +/- is specified, then digits HAVE to follow because it has to be an exponential notation
                        if (One(c => c == '+' || Current == '-') && !Any(char.IsDigit))
                        {
                            throw new Exception("Invalid exponential notation at position {0}".FormatString(_index));
                        }
                        else
                        {
                            Any(char.IsDigit);
                        }
                    }

                    Any(char.IsLetter); // Trailing letters are included
                    break;
                case TokenType.Identifier:
                    Any(c => char.IsLetterOrDigit(c) || c == '_');
                    break;
            }
        }

        /// <summary>
        /// Resets the lexer to the start of the input.
        /// </summary>
        public void Reset()
        {
            _index = 0;
            _builder.Clear();
        }

        /// <summary>
        /// Reads the next node.
        /// </summary>
        public void ReadNode()
        {
            _builder.Clear();
            Token = TokenType.Node;

            // Skip spaces
            while (Current == ' ')
            {
                _index++;
            }

            // Nothing left to read!
            if (Current == '\0')
            {
                Token = TokenType.EndOfExpression;
                return;
            }

            if (Current == '\r' || Current == '\n')
            {
                Token = TokenType.EndOfExpression;
                return;
            }

            // Nodes can be anything except a few characters
            Any(c =>
            {
                switch (c)
                {
                    case ' ':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case ',':
                        return false;
                    default:
                        return true;
                }
            });
            return;
        }

        /// <summary>
        /// Tries to read any characters that match the predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// <c>true</c> if at one or more characters were consumed; otherwise <c>false</c>.
        /// </returns>
        private bool Any(Func<char, bool> predicate)
        {
            bool result = false;
            char c;
            while ((c = Current) != '\0' && predicate(c))
            {
                _builder.Append(c);
                _index++;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Tries to read a character that matches the predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// <c>true</c> if a character was consumed; otherwise <c>false</c>.
        /// </returns>
        private bool One(Func<char, bool> predicate)
        {
            var c = Current;
            if (c == '\0')
            {
                return false;
            }

            if (predicate(c))
            {
                _builder.Append(c);
                _index++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Consumes the current character..
        /// </summary>
        private void Consume()
        {
            _builder.Append(Current);
            _index++;
        }
    }
}
