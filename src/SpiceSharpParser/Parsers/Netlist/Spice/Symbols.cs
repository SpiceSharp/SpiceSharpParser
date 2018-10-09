namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// Contains all non-terminals names for SPICE grammar from "SpiceGrammarBNF.txt".
    /// </summary>
    public class Symbols
    {
        public const string Netlist = "NETLIST";
        public const string NetlistWithoutTitle = "NETLIST_WITHOUT_TITLE";
        public const string NetlistEnding = "NETLIST_ENDING";
        public const string Statements = "STATEMENTS";
        public const string Statement = "STATEMENT";
        public const string Component = "COMPONENT";
        public const string Subckt = "SUBCKT";
        public const string SubcktEnding = "SUBCKT-ENDING";
        public const string Model = "MODEL";
        public const string Control = "CONTROL";
        public const string CommentLine = "COMMENT-LINE";
        public const string Parameters = "PARAMETERS";
        public const string Parameter = "PARAMETER";
        public const string ParameterSingle = "PARAMETER-SINGLE";
        public const string Vector = "VECTOR";
        public const string VectorContinue = "VECTOR-CONTINUE";
        public const string ParameterEqual = "PARAMETER_EQUAL";
        public const string ParameterEqualSingle = "PARAMETER_EQUAL_SINGLE";
        public const string ParameterBracket = "PARAMETER_BRACKET";
        public const string ParameterBracketContent = "PARAMETER_BRACKET_CONTENT";
        public const string NewLine = "NEW-LINE";
    }
}
