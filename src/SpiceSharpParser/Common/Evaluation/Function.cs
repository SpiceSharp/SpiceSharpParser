using System;

namespace SpiceSharpParser.Common.Evaluation
{
    public abstract class Function<TInputArgumentType, TOutputType> : IFunction<TInputArgumentType, TOutputType>
    {
        /// <summary>
        /// Gets or sets the name of function.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets arguments count.
        /// </summary>
        public int ArgumentsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether function is infix.
        /// </summary>
        public bool Infix { get; set; }

        /// <summary>
        /// Computes the value of the function for given arguments.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="args"></param>
        /// <param name="evaluator"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract TOutputType Logic(string image, TInputArgumentType[] args, IEvaluator evaluator, ExpressionContext context);

        /// <summary>
        /// Gets the argument type.
        /// </summary>
        Type IFunction.ArgumentType => typeof(TInputArgumentType);

        /// <summary>
        /// Gets the output type.
        /// </summary>
        Type IFunction.OutputType => typeof(TOutputType);
    }
}
