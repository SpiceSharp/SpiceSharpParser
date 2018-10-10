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
        /// Gets custom functions.
        /// </summary>
        public Dictionary<string, Function> Functions { get; set; }


        public IEvaluator Evaluator { get; set; }
    }
}
