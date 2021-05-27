using System;

namespace SpiceSharpParser.Lexers.Expressions
{
    /// <summary>
    /// A lexer that can tokenize Spice expressions.
    /// </summary>
    /// <remarks>
    /// Code from SpiceSharpBehavioral.
    /// </remarks>
    public class Lexer
    {
        private readonly string _expression;

        /// <summary>
        /// Creates a lexer for a Spice behavioral expression.
        /// </summary>
        public static Lexer FromString(string expression)
            => new(expression);

        /// <summary>
        /// Gets the current token type.
        /// </summary>
        public TokenType Type { get; private set; }

        /// <summary>
        /// Gets the contents of the token.
        /// </summary>
        public string Content => Length == 0 ? "" : _expression.Substring(Index - Length, Length);

        /// <summary>
        /// Gets the current index in the expression.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the length of the token.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        private Lexer(string expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Index = 0;
            Next();
        }

        /// <summary>
        /// Go to the next token.
        /// </summary>
        public void Next()
        {
            Length = 0;
            if (Index >= _expression.Length)
            {
                Type = TokenType.EndOfExpression;
                return;
            }

            // Skip any spaces
            char c = _expression[Index];
            if (c == ' ')
            {
                Index++;
                while (Index < _expression.Length && (c = _expression[Index]) == ' ')
                    Index++;
            }

            // End of the expression
            if (Index >= _expression.Length)
            {
                Type = TokenType.EndOfExpression;
                return;
            }

            // Please forgive the spaghetti code for number parsing...
            switch (c)
            {
                case '+': Type = TokenType.Plus; Continue(); break;
                case '-': Type = TokenType.Minus; Continue(); break;
                case '*':
                    Type = TokenType.Times;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '*')
                    {
                        Type = TokenType.Power;
                        Continue();
                    }
                    break;
                case '/': Type = TokenType.Divide; Continue(); break;
                case '%': Type = TokenType.Mod; Continue(); break;
                case '^': Type = TokenType.Power; Continue(); break;
                case '?': Type = TokenType.Huh; Continue(); break;
                case ':': Type = TokenType.Colon; Continue(); break;
                case '(': Type = TokenType.LeftParenthesis; Continue(); break;
                case ')': Type = TokenType.RightParenthesis; Continue(); break;
                case '[': Type = TokenType.LeftIndex; Continue(); break;
                case ']': Type = TokenType.RightIndex; Continue(); break;
                case '!':
                    Type = TokenType.Bang;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '=')
                    {
                        Type = TokenType.NotEquals;
                        Continue();
                    }
                    break;
                case ',': Type = TokenType.Comma; Continue(); break;
                case '<':
                    Type = TokenType.LessThan;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '=')
                    {
                        Type = TokenType.LessEqual;
                        Continue();
                    }
                    break;
                case '>':
                    Type = TokenType.GreaterThan;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '=')
                    {
                        Type = TokenType.GreaterEqual;
                        Continue();
                    }
                    break;
                case '=':
                    Type = TokenType.Assign;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '=')
                    {
                        Type = TokenType.Equals;
                        Continue();
                    }
                    break;
                case '&':
                    Type = TokenType.Unknown;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '&')
                    {
                        Type = TokenType.And;
                        Continue();
                    }
                    Continue();
                    break;
                case '|':
                    Type = TokenType.Unknown;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] == '|')
                    {
                        Type = TokenType.Or;
                        Continue();
                    }
                    break;
                case '@': Type = TokenType.At; Continue(); break;
                case '.':
                    Type = TokenType.Dot;
                    Continue();
                    if (Index < _expression.Length && _expression[Index] >= '0' && _expression[Index] <= '9')
                    {
                        Type = TokenType.Number;
                        Continue();
                        while (Index < _expression.Length && char.IsDigit(_expression[Index]))
                            Continue();
                        goto caseNumberPostfix;
                    }
                    break;
                case char number when char.IsDigit(number):
                    Type = TokenType.Number;
                    Continue();
                    while (Index < _expression.Length && char.IsDigit(_expression[Index]))
                        Continue();
                    if (Index < _expression.Length && _expression[Index] == '.')
                    {
                        Continue();
                        while (Index < _expression.Length && char.IsDigit(_expression[Index]))
                            Continue();
                    }

                    caseNumberPostfix:
                    // Exponential notation
                    if (Index < _expression.Length && ((number = _expression[Index]) == 'e' || number == 'E'))
                    {
                        Continue();
                        if (Index < _expression.Length && ((number = _expression[Index]) == '+' || number == '-'))
                            Continue();
                        while (Index < _expression.Length && char.IsDigit(_expression[Index]))
                            Continue();
                    }

                    // Just any subsequent stuff
                    while (Index < _expression.Length && (
                        (number = _expression[Index]) >= 'a' && number <= 'z' ||
                        number >= 'A' && number <= 'Z'))
                        Continue();
                    break;
                case char letter when letter >= 'a' && letter <= 'z' || letter >= 'A' && letter <= 'Z' || letter == '_':
                    Type = TokenType.Identifier;
                    Continue();
                    while ((Index < _expression.Length) && ((
                        letter = _expression[Index]) >= 'a' && letter <= 'z' ||
                        letter >= 'A' && letter <= 'Z' ||
                        letter >= '0' && letter <= '9' ||
                        letter == '_'))
                        Continue();
                    break;

                default:
                    Type = TokenType.Unknown;
                    Continue();
                    break;
            }
        }

        /// <summary>
        /// Keep reading the node.
        /// </summary>
        public void ContinueWhileNode()
        {
            Type = TokenType.Node;
            if (Index >= _expression.Length)
                return;

            // Read the node contents
            char c;
            while (Index < _expression.Length &&
                (c = _expression[Index]) != ' ' &&
                    c != '(' && c != ')' &&
                    c != '[' && c != ']' &&
                    c != ',')
                Continue();
        }

        /// <summary>
        /// Go to the next character.
        /// </summary>
        private void Continue()
        {
            Index++;
            Length++;
        }
    }
}
