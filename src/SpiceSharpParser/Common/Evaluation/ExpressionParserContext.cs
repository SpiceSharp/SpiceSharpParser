using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionParserContext
    {
        public ExpressionParserContext() 
            : this(false, false)
        {
        }

        public ExpressionParserContext(bool caseSensitiveParameters, bool caseSensitiveFunctions)
        {
            Parameters = new Dictionary<string, Expression>(StringComparerFactory.Create(caseSensitiveParameters));
            Functions = new Dictionary<string, Function>(StringComparerFactory.Create(caseSensitiveFunctions));
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public Dictionary<string, Expression> Parameters { get; set; }

        /// <summary>
        /// Gets custom functions.
        /// </summary>
        public Dictionary<string, Function> Functions { get; set; }

        /// <summary>
        /// Gets or sets the evaluator.
        /// </summary>
        public IEvaluator Evaluator { get; set; }
    }
}
