using SpiceSharpParser.Lexer.Spice;

namespace SpiceSharpParser.Parser.Spice
{
    /// <summary>
    /// The terminal parse tree node evaluation value
    /// </summary>
    public class ParseTreeNodeTerminalTranslationValue : ParseTreeNodeEvaluationValue
    {
        /// <summary>
        /// Gets or sets value of terminal node
        /// </summary>
        public SpiceToken Token { get; set; }
    }
}
