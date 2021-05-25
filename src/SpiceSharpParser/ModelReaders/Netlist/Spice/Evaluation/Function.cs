namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
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
        /// Computes the value of the function for given arguments.
        /// </summary>
        public abstract TOutputType Logic(string image, TInputArgumentType[] args, EvaluationContext context);
    }
}