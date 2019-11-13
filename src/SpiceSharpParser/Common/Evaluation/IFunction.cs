using System;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Common.Evaluation
{
    public interface IFunction<in TInputArgumentType, out TOutputType> : IFunction
    {
        TOutputType Logic(string image, TInputArgumentType[] args, IEvaluator evaluator, ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null);
    }

    public interface IDerivativeFunction<TInputArgumentType, out TOutputType> : IFunction<TInputArgumentType, TOutputType>
    {
        Derivatives<Func<double>> Derivative(string image, Func<TInputArgumentType>[] args, IEvaluator evaluator,
            ExpressionContext context, Simulation simulation = null, IReadingContext readingContext = null);
    }

    public interface IFunction
    {
        int ArgumentsCount { get; set; }

        string Name { get; set; }
    }
}