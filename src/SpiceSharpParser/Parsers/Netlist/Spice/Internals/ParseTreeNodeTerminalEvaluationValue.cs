using SpiceSharpParser.Lexers.Netlist.Spice;

namespace SpiceSharpParser.Parsers.Netlist.Spice.Internals
{
    /// <summary>
    /// The terminal parse tree node evaluation value.
    /// </summary>
    public class ParseTreeNodeTerminalEvaluationValue : ParseTreeNodeEvaluationValue
    {
        /// <summary>
        /// Gets or sets value of terminal node.
        /// </summary>
        public SpiceToken Token { get; set; }
    }
}