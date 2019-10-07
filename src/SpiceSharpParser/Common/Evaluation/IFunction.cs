using System;
using SpiceSharpBehavioral.Parsers;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IFunction<in TInputArgumentType, out TOutputType> : IFunction
    {
        TOutputType Logic(string image, TInputArgumentType[] args, IEvaluator evaluator, ExpressionContext context);
    }

    public interface IDerivativeFunction<in TInputArgumentType, out TOutputType> : IFunction<TInputArgumentType, TOutputType>
    {
        Derivatives<Func<double>> Derivative(string image, TInputArgumentType[] args, IEvaluator evaluator, ExpressionContext context);
    }

    public interface IFunction
    {
        int ArgumentsCount { get; set; }

        bool Infix { get; set; }

        string Name { get; set; }

        System.Type ArgumentType { get; }

        System.Type OutputType { get; }
    }
}