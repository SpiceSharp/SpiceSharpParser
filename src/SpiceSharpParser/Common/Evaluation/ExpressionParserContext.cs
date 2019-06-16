using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionParserContext
    {
        public ExpressionParserContext()
            : this(string.Empty, false)
        {
        }

        public ExpressionParserContext(string name, bool caseSensitiveFunctions)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Functions = new Dictionary<string, List<IFunction>>(StringComparerProvider.Get(caseSensitiveFunctions));
        }

        public ExpressionParserContext(string name, Dictionary<string, List<IFunction>> functions)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Functions = functions ?? throw new ArgumentNullException(nameof(functions));
        }

        public ExpressionParserContext(Dictionary<string, List<IFunction>> functions) 
            : this(string.Empty, functions)
        {
            if (functions == null)
            {
                throw new ArgumentNullException(nameof(functions));
            }
        }

        /// <summary>
        /// Gets or sets custom functions.
        /// </summary>
        public Dictionary<string, List<IFunction>> Functions { get; protected set; }

        /// <summary>
        /// Gets or sets the name of parser context.
        /// </summary>
        public string Name { get; protected set; }
    }
}
