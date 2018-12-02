using System;

namespace SpiceSharpParser.Lexers
{
    public class LexerDynamicRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerDynamicRule"/> class.
        /// </summary>
        public LexerDynamicRule(int tokenType, string ruleName, string prefix, Func<string, string> action)
        {
            TokenType = tokenType;
            RuleName = ruleName;
            Prefix = prefix;
            Action = action;
        }

        public int TokenType { get; }

        public string RuleName { get; }

        public string Prefix { get; }

        public Func<string, string> Action { get; }
    }
}
