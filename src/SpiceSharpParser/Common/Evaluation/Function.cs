using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public class Function
    {
        /// <summary>
        /// Gets or sets the name of user function.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets logic for user function.
        /// Function:
        /// name => args = => evaluator => result.
        /// </summary>
        public Func<string, object[], IEvaluator, object> Logic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether function has virtual parameters.
        /// </summary>
        public bool VirtualParameters { get; set; } = true;

        /// <summary>
        /// Gets or sets arguments count.
        /// </summary>
        public int ArgumentsCount { get; set; }

        // TODO: future: add validation

        /// <summary>
        /// Gets or sets the return type of user function .
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether function is infix.
        /// </summary>
        public bool Infix { get; set; }
    }
}
