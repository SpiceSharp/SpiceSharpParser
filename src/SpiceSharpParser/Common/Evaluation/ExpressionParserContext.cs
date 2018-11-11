using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionParserContext
    {
        public ExpressionParserContext()
            : this(false)
        {
        }

        public ExpressionParserContext(bool caseSensitiveFunctions)
        {
            Functions = new Dictionary<string, Function>(StringComparerProvider.Get(caseSensitiveFunctions));
        }

        /// <summary>
        /// Gets or sets custom functions.
        /// </summary>
        public Dictionary<string, Function> Functions { get; set; }

        /// <summary>
        /// Gets or sets the name of parser context.
        /// </summary>
        public string Name { get; set; }
    }
}
