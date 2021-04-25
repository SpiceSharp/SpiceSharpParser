using System.Text.RegularExpressions;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// A base class for lexer state classes. It contains a type of previous token.
    /// </summary>
    public class LexerState
    {
        /// <summary>
        /// Gets or sets type of previously returned token by lexer.
        /// </summary>
        public int PreviousReturnedTokenType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lexem is a full match.
        /// </summary>
        public bool FullMatch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lexem is before a line break character.
        /// </summary>
        public bool BeforeLineBreak { get; set; }

        /// <summary>
        /// Gets or sets the current line number.
        /// </summary>
        public int LineNumber { get; set; } = 0;

        /// <summary>
        /// Gets or sets the start column index.
        /// </summary>
        public int StartColumnIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the lexer is lexing new line.
        /// </summary>
        public bool NewLine { get; set; } = true;

        /// <summary>
        /// Gets or sets the current regex rule.
        /// </summary>
        public Regex CurrentRuleRegex { get; set; }
    }
}