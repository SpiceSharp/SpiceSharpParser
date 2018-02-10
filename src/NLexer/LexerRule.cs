using System.Text.RegularExpressions;

namespace NLexer
{
    public abstract class LexerRule
    {
        private Regex regex;
        private string regularExpressionPattern;

        /// <summary>
        /// Name of lexer rule
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Regular expression of lexer rule
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
        /// Regex for the lexer rule regular's expression
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

        public LexerRule(string name, string regularExpressionPattern)
        {
            RegularExpressionPattern = regularExpressionPattern;
            Name = name;
        }

        internal abstract LexerRule Clone();
    }
}
