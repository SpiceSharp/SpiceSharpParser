using SpiceSharpParser.SpiceLexer;

namespace SpiceSharpParser.Parser.Translation
{
    /// <summary>
    /// The terminal parse tree node evaluation value
    /// </summary>
    public class ParseTreeNodeTerminalTranslationValue : ParseTreeNodeTranslationValue
    {
        /// <summary>
        /// Gets or sets value of terminal node
        /// </summary>
        public SpiceToken Token { get; set; }
    }
}
