using System;
using System.Text.RegularExpressions;

namespace SpiceSharpParser.Lexers
{
    /// <summary>
    /// Base class for token rules in <see cref="LexerGrammar{TLexerState}"/>.
    /// </summary>
    public abstract class LexerRegexRule
    {
        private Regex _regex;
        private string _regularExpressionPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerRegexRule"/> class.
        /// </summary>
        /// <param name="ruleName">A name of lexer rule.</param>
        /// <param name="regularExpressionPattern">A regular expression.</param>
        /// <param name="ignoreCase">Case is ignored.</param>
        protected LexerRegexRule(string ruleName, string regularExpressionPattern, bool ignoreCase)
        {
            Name = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
            RegularExpressionPattern = regularExpressionPattern ?? throw new ArgumentNullException(nameof(regularExpressionPattern));
            IgnoreCase = ignoreCase;
        }

        /// <summary>
        /// Gets a value indicating whether case of characters in regular expression is ignored.
        /// </summary>
        public bool IgnoreCase { get; }

        /// <summary>
        /// Gets name of lexer rule.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a regular expression pattern of lexer rule.
        /// </summary>
        public string RegularExpressionPattern
        {
            get => _regularExpressionPattern;

            set
            {
                _regularExpressionPattern = value;
                _regex = null;
            }
        }

        /// <summary>
        /// Gets a regular expression of lexer rule.
        /// </summary>
        public Regex RegularExpression
        {
            get
            {
                if (_regex == null)
                {
                    RegexOptions options = RegexOptions.None;

                    if (IgnoreCase)
                    {
                        options |= RegexOptions.IgnoreCase;
                    }

                    _regex = new Regex($"^{RegularExpressionPattern}", options);
                }

                return _regex;
            }
        }

        /// <summary>
        /// Clones the rule.
        /// </summary>
        /// <returns>
        /// Clone of the rule.
        /// </returns>
        public abstract LexerRegexRule Clone();
    }
}