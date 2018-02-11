using System;

namespace NLexer
{
    public class Token
    {
        public Token(int tokenType, string value)
        {
            TokenType = tokenType;
            Value = value;
        }

        /// <summary>
        /// Gets token type
        /// </summary>
        public int TokenType { get; }

        /// <summary>
        /// Gets token value
        /// </summary>
        public string Value { get; private set; }

        public int TokenLength
        {
            get
            {
                if (Value != null)
                {
                    return Value.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void UpdateValue(string value)
        {
            this.Value = value;
        }
    }
}
