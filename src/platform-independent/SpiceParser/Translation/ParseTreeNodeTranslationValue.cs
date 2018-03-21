using SpiceParser.Parsing;

namespace SpiceParser.Translation
{
    /// <summary>
    /// The tree node evaluation value
    /// </summary>
    public abstract class ParseTreeNodeTranslationValue
    {
        /// <summary>
        /// Gets or sets reference to the parse tree node
        /// </summary>
        public ParseTreeNode Node { get; set; }
    }
}
