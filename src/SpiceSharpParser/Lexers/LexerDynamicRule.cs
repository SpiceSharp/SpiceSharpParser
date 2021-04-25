using System;
using System.Text.RegularExpressions;

namespace SpiceSharpParser.Lexers
{
    public class LexerDynamicRule
    {
        private Regex _regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerDynamicRule"/> class.
        /// </summary>
        public LexerDynamicRule(int tokenType, string ruleName, string prefix, Func<string, LexerState, Tuple<string, int>> action, int[] previousReturnedTokenTypes = null)
        {
            TokenType = tokenType;
            RuleName = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            PreviousReturnedTokenTypes = previousReturnedTokenTypes;
        }

        public int TokenType { get; }

        public string RuleName { get; }

        public string Prefix { get; }

        /// <summary>
        /// Gets a regular expression of lexer rule.
        /// </summary>
        public Regex PrefixExpression
        {
            get
            {
                if (_regex == null)
                {
                    RegexOptions options = RegexOptions.None;
                    options |= RegexOptions.IgnoreCase;
                    _regex = new Regex($"^{Prefix}", options);
                }

                return _regex;
            }
        }

        public Func<string, LexerState, Tuple<string, int>> Action { get; }

        public int[] PreviousReturnedTokenTypes { get; }
    }
}