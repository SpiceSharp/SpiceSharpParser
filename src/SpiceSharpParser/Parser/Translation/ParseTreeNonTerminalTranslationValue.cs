using SpiceSharpParser.Model;

namespace SpiceSharpParser.Parser.Translation
{
    /// <summary>
    /// The non-terminal tree node evaluation value
    /// </summary>
    public class ParseTreeNonTerminalTranslationValue : ParseTreeNodeTranslationValue
    {
        /// <summary>
        /// Gets or sets the value of non-terminal node
        /// </summary>
        public SpiceObject SpiceObject { get; set; }
    }
}
