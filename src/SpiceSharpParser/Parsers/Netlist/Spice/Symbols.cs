namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// Contains all non-terminals names for SPICE grammar from "SpiceGrammarBNF.txt".
    /// </summary>
    public class Symbols
    {
        public const string Netlist = "NETLIST";
        public const string NetlistWithoutTitle = "NETLIST-WITHOUT-TITLE";
        public const string NetlistEnding = "NETLIST-ENDING";
        public const string Statements = "STATEMENTS";
        public const string Statement = "STATEMENT";
        public const string Component = "COMPONENT";
        public const string Subckt = "SUBCKT";
        public const string SubcktEnding = "SUBCKT-ENDING";
        public const string Model = "MODEL";
        public const string Control = "CONTROL";
        public const string CommentLine = "COMMENT-LINE";
        public const string Parameters = "PARAMETERS";
        public const string ParametersSeparator = "PARAMETERS-SEPARATOR";
        public const string Parameter = "PARAMETER";
        public const string ParameterSingle = "PARAMETER-SINGLE";
        public const string Vector = "VECTOR";
        public const string VectorContinue = "VECTOR-CONTINUE";
        public const string ParameterEqual = "PARAMETER-EQUAL";
        public const string ParameterEqualSingle = "PARAMETER-EQUAL-SINGLE";
        public const string ParameterBracket = "PARAMETER-BRACKET";
        public const string ParameterBracketContent = "PARAMETER-BRACKET-CONTENT";
        public const string NewLine = "NEW-LINE";
        public const string NewLines = "NEW-LINES";
        public const string Points = "POINTS";
        public const string PointsContinue = "POINTS-CONTINUE";
        public const string Point = "POINT";
        public const string PointValue = "POINT-VALUE";
        public const string PointValues = "POINT-VALUES";
        public const string ExpressionEqual = "EXPRESSION-EQUAL";
        public const string Distribution = "DISTRIBUTION";
    }
}
