using System;

namespace SpiceSharpParser.Lexers
{
    public class LexerDynamicRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexerDynamicRule"/> class.
        /// </summary>
        public LexerDynamicRule(int tokenType, string ruleName, string prefix, Func<string, LexerState, Tuple<string, int>> action)
        {
            TokenType = tokenType;
            RuleName = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public int TokenType { get; }

        public string RuleName { get; }

        public string Prefix { get; }

        public Func<string, LexerState, Tuple<string, int>> Action { get; }
    }
}