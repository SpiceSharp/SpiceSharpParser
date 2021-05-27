namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public interface IFunction<in TInputArgumentType, out TOutputType> : IFunction
    {
        TOutputType Logic(string image, TInputArgumentType[] args, EvaluationContext context);
    }

    public interface IFunction
    {
        int ArgumentsCount { get; set; }

        string Name { get; set; }
    }
}