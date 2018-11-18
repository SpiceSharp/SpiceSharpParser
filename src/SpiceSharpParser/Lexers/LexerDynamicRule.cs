using System;

namespace SpiceSharpParser.Lexers
{
    public class LexerDynamicRule
    {
        public int TokenType { get; }

        public string RuleName { get; }

        public string Prefix { get; }

        public Func<string, string> Action { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerRegexRule"/> class.
        /// </summary>
        public LexerDynamicRule(int tokenType, string ruleName, string prefix, Func<string,string> action)
        {
            this.TokenType = tokenType;
            this.RuleName = ruleName;
            this.Prefix = prefix;
            this.Action = action;
        }
    }
}
