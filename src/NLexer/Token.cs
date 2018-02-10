namespace NLexer
{
    public class Token
    {
        public int TokenType { get; set; }

        public string Value { get; set; }

        public int TokenLength
        {
            get
            {
                if (Value != null)
                {
                    return Value.Length;
                }
                else return 0;
            }
        }
    }
}
