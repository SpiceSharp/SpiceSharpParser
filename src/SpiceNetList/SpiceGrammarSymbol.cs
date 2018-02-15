namespace SpiceNetlist
{
    public class SpiceGrammarSymbol
    {
        public const string START = "START";
        public const string STATEMENTS = "STATEMENTS";
        public const string STATEMENT = "STATEMENT";
        public const string COMPONENT = "COMPONENT";
        public const string SUBCKT = "SUBCKT";
        public const string SUBCKT_ENDING = "SUBCKT-ENDING";
        public const string NEW_LINE_OR_EOF = "NEW-LINE-OR-EOF";
        public const string MODEL = "MODEL";
        public const string CONTROL = "CONTROL";
        public const string COMMENT_LINE = "COMMENT-LINE";
        public const string PARAMETERS = "PARAMETERS";
        public const string PARAMETER = "PARAMETER";
        public const string PARAMETER_SINGLE = "PARAMETER-SINGLE";
        public const string VECTOR = "VECTOR";
        public const string VECTOR_CONTINUE = "VECTOR-CONTINUE";
        public const string BRACKET_CONTENT = "BRACKET_CONTENT";
    }
}
