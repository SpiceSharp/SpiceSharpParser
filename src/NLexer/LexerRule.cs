using System.Text.RegularExpressions;

namespace NLexer
{
    /// <summary>
    /// Base class for token rules in <see cref="LexerGrammar{TLexerState}"/>.
    /// </summary>
    public abstract class LexerRule
    {
        private Regex regex;
        private string regularExpressionPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerRule"/> class.
        /// </summary>
        /// <param name="ruleName">A name of lexer rule</param>
        /// <param name="regularExpressionPattern">A regular expression</param>
        public LexerRule(string ruleName, string regularExpressionPattern)
        {
            RegularExpressionPattern = regularExpressionPattern;
            Name = ruleName;
        }

        /// <summary>
        /// Gets name of lexer rule
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a regular expression pattern of lexer rule
        /// </summary>
        public string RegularExpressionPattern
        {
            get
            {
                return this.regularExpressionPattern;
            }

            set
            {
                this.regularExpressionPattern = value;
                this.regex = null;
            }
        }

        /// <summary>
        /// Gets a regular expression of lexer rule
        /// </summary>
        public Regex RegularExpression
        {
            get
            {
                if (regex == null)
                {
                    regex = new Regex("^" + RegularExpressionPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }

                return regex;
            }
        }

        /// <summary>
        /// Clones the rule
        /// </summary>
        /// <returns>
        /// Clone of the rule
        /// </returns>
        public abstract LexerRule Clone();
    }
}
