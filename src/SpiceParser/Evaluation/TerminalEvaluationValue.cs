using NLexer;

namespace SpiceParser.Evaluation
{
    /// <summary>
    /// The terminal parse tree node evaluation value
    /// </summary>
    public class TerminalEvaluationValue : EvaluationValue
    {
        /// <summary>
        /// Gets or sets value of terminal node
        /// </summary>
        public Token Token { get; set; }
    }
}
