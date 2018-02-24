namespace SpiceGrammar
{
    /// <summary>
    /// Contains all non-terminals names for spice grammar from "SpiceBNF.txt"
    /// </summary>
    public class SpiceGrammarSymbol
    {
        public const string START = "START";
        public const string STATEMENTS = "STATEMENTS";
        public const string STATEMENT = "STATEMENT";
        public const string COMPONENT = "COMPONENT";
        public const string SUBCKT = "SUBCKT";
        public const string SUBCKT_ENDING = "SUBCKT-ENDING";
        public const string MODEL = "MODEL";
        public const string CONTROL = "CONTROL";
        public const string COMMENT_LINE = "COMMENT-LINE";
        public const string PARAMETERS = "PARAMETERS";
        public const string PARAMETER = "PARAMETER";
        public const string PARAMETER_SINGLE = "PARAMETER-SINGLE";
        public const string PARAMETER_SINGLE_SEQUENCE = "PARAMETER-SINGLE-SEQUENCE";
        public const string PARAMETER_SINGLE_SEQUENCE_CONTINUE = "PARAMETER-SINGLE-SEQUENCE-CONTINUE";
        public const string VECTOR = "VECTOR";
        public const string VECTOR_CONTINUE = "VECTOR-CONTINUE";
        public const string PARAMETER_EQUAL = "PARAMETER_EQUAL";
        public const string PARAMETER_EQUAL_SINGLE = "PARAMETER_EQUAL_SINGLE";
        public const string PARAMETER_EQUAL_SEQUANCE = "PARAMETER_EQUAL_SEQUENCE";
        public const string PARAMETER_EQUAL_SEQUANCE_CONTINUE = "PARAMETER_EQUAL_SEQUENCE_CONTINUE";
        public const string PARAMETER_BRACKET = "PARAMETER_BRACKET";
        public const string PARAMETER_BRACKET_CONTENT = "PARAMETER_BRACKET_CONTENT";
        public const string NEW_LINE_OR_EOF = "NEW-LINE-OR-EOF";
    }
}
