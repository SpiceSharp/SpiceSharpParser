using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ParsersHelpers
    {
        public static SimpleParser GetSimpleParser(ExpressionContext context, IEvaluator evaluator)
        {
            var parser = new SimpleParser();
            parser.RegisterDefaultFunctions();

            parser.VariableFound += (sender, args) =>
            {
                if (context.Parameters.ContainsKey(args.Name))
                {
                    args.Result = GetParameter(evaluator, context, args.Name);
                }
            };

            parser.FunctionFound += (sender, args) =>
            {
                if (args.Found)
                {
                    return;
                }

                if (context.Functions.TryGetValue(args.Name, out var functions))
                {
                    if (functions.First() is IFunction<double, double> function)
                    {
                        var arguments = new List<double>();
                        for (var i = 0; i < function.ArgumentsCount; i++)
                        {
                            arguments.Add(args[i]);
                        }

                        args.Result = function.Logic(
                            string.Empty,
                            arguments.ToArray(),
                            evaluator,
                            context);
                    }
                }
            };

            return parser;
        }

        public static SimpleDerivativeParser GetDeriveParser(ExpressionContext context, IEvaluator evaluator, List<string> argumentNames, double[] argumentValues)
        {
            var parser = new SimpleDerivativeParser();
            parser.RegisterDefaultFunctions();

            parser.VariableFound += (sender, args) =>
            {
                if (argumentNames.Contains(args.Name))
                {
                    var d = new DoubleDerivatives(2);
                    d[0] = () => argumentValues[argumentNames.IndexOf(args.Name)];
                    d[1] = () => 1;
                    args.Result = d;
                    return;
                }

                if (context.Parameters.ContainsKey(args.Name))
                {
                    var d = new DoubleDerivatives(2);
                    d[0] = () => GetParameter(evaluator, context, args.Name);
                    d[1] = () => 0;
                    args.Result = d;
                }
            };
            parser.FunctionFound += GetFunctionFound(context, evaluator);

            return parser;
        }

        public static SimpleDerivativeParser GetDeriveParser(IReadingContext context)
        {
            var parser = new SimpleDerivativeParser();
            parser.RegisterDefaultFunctions();

            parser.VariableFound += (sender, args) =>
            {
                var simulationExpressionContext = context.SimulationExpressionContexts.GetContext(context.Result.Simulations.First());
                var evaluator = context.SimulationEvaluators.GetEvaluator(context.Result.Simulations.First());

                if (simulationExpressionContext.Parameters.ContainsKey(args.Name))
                {
                    var d = new DoubleDerivatives(2);
                    d[0] = () => GetParameter(evaluator, simulationExpressionContext, args.Name);
                    d[1] = () => 0;
                    args.Result = d;
                    return;
                }

                var readingContext = context.ReadingExpressionContext;

                if (readingContext.Parameters.ContainsKey(args.Name))
                {
                    var d = new DoubleDerivatives(2);
                    d[0] = () => GetParameter(evaluator, readingContext, args.Name);
                    d[1] = () => 0;
                    args.Result = d;
                }
            };

            parser.FunctionFound += GetFunctionFound(context);

            return parser;
        }

        private static EventHandler<FunctionFoundEventArgs<Derivatives<Func<double>>>> GetFunctionFound(ExpressionContext context, IEvaluator evaluator)
        {
            return (sender, args) =>
            {
                if (args.Found)
                {
                    return;
                }

                if (context.Functions.TryGetValue(args.Name, out var functions))
                {
                    if (functions.First() is Function<double, double> function)
                    {
                        var argumentCount = args.ArgumentCount;

                        var variableCount = 1;
                        for (var i = 0; i < argumentCount; i++)
                        {
                            variableCount = Math.Max(variableCount, args[i].Count);
                        }

                        var result = new DoubleDerivatives(variableCount);

                        var arguments = new List<Func<double>>();
                        for (var i = 0; i < argumentCount; i++)
                        {
                            var j = i;
                            arguments.Add(() => args[j][0]());
                        }

                        result[0] = () =>
                        {
                            var fArgs = arguments.Select(arg => arg()).ToArray();

                            return function.Logic(
                                string.Empty,
                                fArgs,
                                evaluator,
                                context);
                        };

                        if (function is IDerivativeFunction<double, double> derivativeFunction)
                        {
                            for (var variableIndex = 1; variableIndex <= variableCount; variableIndex++)
                            {
                                var j = variableIndex;

                                if (IsDerivativeDefined(args, variableIndex))
                                {
                                    result[variableIndex] = () => GetDerivativeValue(
                                        context,
                                        evaluator,
                                        args,
                                        j,
                                        derivativeFunction,
                                        arguments,
                                        argumentCount);
                                }
                            }
                        }

                        args.Result = result;
                    }
                }
            };
        }

        private static EventHandler<FunctionFoundEventArgs<Derivatives<Func<double>>>> GetFunctionFound(IReadingContext context)
        {
            return (sender, args) =>
            {
                if (args.Found)
                {
                    return;
                }

                if (context.ReadingExpressionContext.Functions.TryGetValue(args.Name, out var functions))
                {
                    if (functions.First() is IFunction<double, double> function)
                    {
                        var argumentCount = args.ArgumentCount;

                        var variableCount = 1;
                        for (var i = 0; i < argumentCount; i++)
                        {
                            variableCount = Math.Max(variableCount, args[i].Count);
                        }

                        var result = new DoubleDerivatives(variableCount);

                        var arguments = new List<Func<double>>();
                        for (var i = 0; i < argumentCount; i++)
                        {
                            var j = i;
                            arguments.Add(() => args[j][0]());
                        }

                        result[0] = () =>
                        {
                            var fArgs = arguments.Select(arg => arg()).ToArray();

                            return function.Logic(
                                string.Empty,
                                fArgs,
                                context.ReadingEvaluator,
                                context.ReadingExpressionContext);
                        };

                        if (function is IDerivativeFunction<double, double> derivativeFunction)
                        {
                            for (var i = 1; i <= variableCount; i++)
                            {
                                var j = i;
                                if (IsDerivativeDefined(args, j))
                                {
                                    result[i] = () => GetDerivativeValue(
                                        context.ReadingExpressionContext,
                                        context.ReadingEvaluator,
                                        args,
                                        j,
                                        derivativeFunction,
                                        arguments,
                                        argumentCount);
                                }
                            }
                        }

                        args.Result = result;
                    }
                }
            };
        }

        private static bool IsDerivativeDefined(FunctionFoundEventArgs<Derivatives<Func<double>>> args, int variableIndex)
        {
            for (var i = 0; i < args.ArgumentCount; i++)
            {
                if (args[i][variableIndex] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static double GetDerivativeValue(ExpressionContext context, IEvaluator evaluator, FunctionFoundEventArgs<Derivatives<Func<double>>> args, int deriveIndex, IDerivativeFunction<double, double> derivativeFunction, List<Func<double>> arguments, int argumentCount)
        {
            var result = 0.0;

            for (var argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
            {
                if (args[argumentIndex][deriveIndex] != null)
                {
                    var derivs = derivativeFunction.Derivative(
                        string.Empty,
                        arguments.Select(arg => arg()).ToArray(),
                        evaluator,
                        context);

                    var deriv = derivs[argumentIndex + 1];

                    if (deriv != null)
                    {
                        result += args[argumentIndex][deriveIndex]() * deriv.Invoke();
                    }
                }
            }

            return result;
        }

        private static double GetParameter(IEvaluator evaluator, ExpressionContext simulationExpressionContext, string argsName)
        {
            var parameter = simulationExpressionContext.Parameters[argsName];
            var val = parameter.Evaluate(evaluator, simulationExpressionContext);

            return val;
        }
    }
}
