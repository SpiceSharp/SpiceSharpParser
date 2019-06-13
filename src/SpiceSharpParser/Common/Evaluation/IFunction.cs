namespace SpiceSharpParser.Common.Evaluation
{
    public interface IFunction<TInputArgumentType, TOutputType> : IFunction
    {
        TOutputType Logic(string image, TInputArgumentType[] args, IEvaluator evaluator, ExpressionContext context);
    }

    public interface IFunction
    {
        int ArgumentsCount { get; set; }

        bool Infix { get; set; }

        string Name { get; set; }

        bool VirtualParameters { get; set; }

        System.Type ArgumentType { get; }

        System.Type OutputType { get; }
    }
}