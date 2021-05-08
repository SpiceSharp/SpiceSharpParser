namespace SpiceSharpParser.Parsers.Netlist.Spice
{
    /// <summary>
    /// Contains all non-terminals names for SPICE grammar from "SpiceGrammarBNF.txt".
    /// </summary>
    public class Symbols
    {
        public static readonly string Netlist = "NETLIST";
        public static readonly string NetlistWithoutTitle = "NETLIST-WITHOUT-TITLE";
        public static readonly string NetlistEnding = "NETLIST-ENDING";
        public static readonly string Statements = "STATEMENTS";
        public static readonly string Statement = "STATEMENT";
        public static readonly string Component = "COMPONENT";
        public static readonly string Subckt = "SUBCKT";
        public static readonly string SubcktEnding = "SUBCKT-ENDING";
        public static readonly string Model = "MODEL";
        public static readonly string Control = "CONTROL";
        public static readonly string CommentLine = "COMMENT-LINE";
        public static readonly string Parameters = "PARAMETERS";
        public static readonly string ParametersSeparator = "PARAMETERS-SEPARATOR";
        public static readonly string Parameter = "PARAMETER";
        public static readonly string ParameterSingle = "PARAMETER-SINGLE";
        public static readonly string Vector = "VECTOR";
        public static readonly string VectorContinue = "VECTOR-CONTINUE";
        public static readonly string ParameterEqual = "PARAMETER-EQUAL";
        public static readonly string ParameterEqualSingle = "PARAMETER-EQUAL-SINGLE";
        public static readonly string ParameterBracket = "PARAMETER-BRACKET";
        public static readonly string ParameterBracketContent = "PARAMETER-BRACKET-CONTENT";
        public static readonly string NewLine = "NEW-LINE";
        public static readonly string NewLines = "NEW-LINES";
        public static readonly string Points = "POINTS";
        public static readonly string PointsContinue = "POINTS-CONTINUE";
        public static readonly string Point = "POINT";
        public static readonly string PointValue = "POINT-VALUE";
        public static readonly string PointValues = "POINT-VALUES";
        public static readonly string ExpressionEqual = "EXPRESSION-EQUAL";
        public static readonly string Distribution = "DISTRIBUTION";
        public static readonly string Parallel = "PARALLEL";
        public static readonly string ParallelEnding = "PARALLEL-ENDING";
    }
}