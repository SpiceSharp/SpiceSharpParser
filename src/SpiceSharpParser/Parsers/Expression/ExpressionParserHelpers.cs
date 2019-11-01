using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Helper;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParserHelpers
    {
        public static double GetExpressionValue(string expression, ExpressionContext context, IEvaluator evaluator, Simulation simulation, IReadingContext readingContext, bool @throw = true)
        {
            var parser = GetDeriveParser(context, readingContext, evaluator, simulation, @throw);
            var derivatives = parser.Parse(expression);
            return derivatives[0]();
        }

        public static List<string> GetExpressionParameters(string expression, ExpressionContext context, IReadingContext readingContext, bool @throw)
        {
            var parser = GetDeriveParser(context, readingContext, null, null, @throw);
            var parameters = new List<string>();
            parser.VariableFound += (sender, e) => 
            {
                if (!parameters.Contains(e.Name))
                {
                    parameters.Add(e.Name);
                }
            };
            parser.SpicePropertyFound += (sender, e) => { e.Apply(() => 0); };

            parser.Parse(expression);
            return parameters;
        }

        public static SimpleDerivativeParser GetDeriveParser(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation, bool @throw = true)
        {
            var parser = new SimpleDerivativeParser();
            parser.RegisterDefaultFunctions();

            parser.VariableFound += (sender, args) =>
            {
                if (context.Arguments.Any(a => a.Key == args.Name))
                {
                    var d = new DoubleDerivatives(2);
                    d[0] = () => context.Arguments.First(a => a.Key == args.Name).Value.Evaluate(evaluator, context,simulation, readingContext);
                    d[1] = () => 1;
                    args.Result = d;
                    return;
                }

                if (context.Parameters.ContainsKey(args.Name))
                {
                    var d = new DoubleDerivatives(2);
                    d[0] = () => context.Parameters[args.Name].Evaluate(evaluator, context, simulation, readingContext);
                    d[1] = () => 0;
                    args.Result = d;
                    return;
                }

                if (readingContext != null)
                {
                    var readingExpressionContext = readingContext.ReadingExpressionContext;

                    if (readingExpressionContext.Parameters.ContainsKey(args.Name))
                    {
                        var d = new DoubleDerivatives(2);
                        d[0] = () => readingExpressionContext.Parameters[args.Name].Evaluate(evaluator, context,simulation, readingContext);
                        d[1] = () => 0;
                        args.Result = d;
                        return;
                    }
                }

                if (@throw)
                {
                    throw new UnknownParameterException();
                }
                else
                {
                    var d = new DoubleDerivatives(1);
                    d[0] = () => double.NaN;
                    args.Result = d;
                }
            };
            parser.FunctionFound += GetFunctionFound(context, readingContext, evaluator, simulation);
            parser.SpicePropertyFound += (sender, arg) =>
            {
                if (simulation == null)
                {
                    arg.Apply(() => 0);
                    return;
                }

                var type = arg.Property.Identifier;
                var vectorParameter = new VectorParameter();
                for (var i = 0; i < arg.Property.ArgumentCount; i++)
                {
                    vectorParameter.Elements.Add(new WordParameter(arg.Property[i].ToString()));
                }

                var voltageExportFactory = new VoltageExporter();
                var currentExportFactory = new CurrentExporter();
                var propertyExporter = new PropertyExporter();
                Exporter factory = null;

                var parameters = new ParameterCollection { vectorParameter };

                if (currentExportFactory.CreatedTypes.Contains(type.ToLower()))
                {
                    factory = currentExportFactory;
                }

                if (voltageExportFactory.CreatedTypes.Contains(type.ToLower()))
                {
                    factory = voltageExportFactory;
                }

                if (type.ToLower() == "@")
                {
                    factory = propertyExporter;
                }

                if (factory == null)
                {
                    throw new Exception("Unknown spice property");
                }

                var export = factory.CreateExport(string.Empty,
                    type,
                    parameters,
                    simulation,
                    readingContext.NodeNameGenerator,
                    readingContext.ComponentNameGenerator,
                    readingContext.ModelNameGenerator,
                    readingContext.Result,
                    readingContext.CaseSensitivity);

                arg.Apply(() => export.Extract());
            };
            return parser;
        }

        private static EventHandler<FunctionFoundEventArgs<Derivatives<Func<double>>>> GetFunctionFound(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation)
        {
            return (sender, args) =>
            {
                if (args.Found)
                {
                    return;
                }

                if (context == null)
                {
                    return;
                }

                if (context.Functions.TryGetValue(args.Name, out var functions))
                {
                    var function = functions.FirstOrDefault(f => f.ArgumentsCount == args.ArgumentCount);

                    if (function == null)
                    {
                        function = functions.FirstOrDefault(f => f.ArgumentsCount == -1);
                    }

                    if (function is IFunction<double, double> doubleFunction)
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

                            return doubleFunction.Logic(
                                string.Empty,
                                fArgs,
                                evaluator ?? readingContext?.ReadingEvaluator,
                                context,
                                simulation,
                                readingContext);
                        };

                        if (function is IDerivativeFunction<double, double> derivativeFunction)
                        {
                            for (var i = 1; i <= variableCount; i++)
                            {
                                var j = i;
                                if (IsDerivativeDefined(args, j))
                                {
                                    result[i] = () => GetDerivativeValue(
                                        context,
                                        evaluator ?? readingContext?.ReadingEvaluator,
                                        args,
                                        j,
                                        derivativeFunction,
                                        arguments,
                                        argumentCount,
                                        simulation,
                                        readingContext);
                                }
                            }
                        }

                        args.Result = result;
                    }
                }

                if (readingContext != null)
                {
                    if (readingContext.ReadingExpressionContext.Functions.TryGetValue(args.Name, out var contextFunctions))
                    {
                        var function = contextFunctions.FirstOrDefault(f => f.ArgumentsCount == args.ArgumentCount);

                        if (function == null)
                        {
                            function = contextFunctions.FirstOrDefault(f => f.ArgumentsCount == -1);
                        }

                        if (function is IFunction<double, double> doubleFunction)
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

                                return doubleFunction.Logic(
                                    string.Empty,
                                    fArgs,
                                    evaluator ?? readingContext.ReadingEvaluator,
                                    context,
                                    simulation,
                                    readingContext);
                            };

                            if (function is IDerivativeFunction<double, double> derivativeFunction)
                            {
                                for (var i = 1; i <= variableCount; i++)
                                {
                                    var j = i;
                                    if (IsDerivativeDefined(args, j))
                                    {
                                        result[i] = () => GetDerivativeValue(
                                            context,
                                            evaluator ?? readingContext.ReadingEvaluator,
                                            args,
                                            j,
                                            derivativeFunction,
                                            arguments,
                                            argumentCount,
                                            simulation,
                                            readingContext);
                                    }
                                }
                            }

                            args.Result = result;
                        }
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

        private static double GetDerivativeValue(ExpressionContext context, IEvaluator evaluator, FunctionFoundEventArgs<Derivatives<Func<double>>> args, int deriveIndex, IDerivativeFunction<double, double> derivativeFunction, List<Func<double>> arguments, int argumentCount, Simulation simulation, IReadingContext readingContext)
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
                        context,
                        simulation,
                        readingContext);

                    var deriv = derivs[argumentIndex + 1];

                    if (deriv != null)
                    {
                        result += args[argumentIndex][deriveIndex]() * deriv.Invoke();
                    }
                }
            }

            return result;
        }

        public static bool HaveSpiceProperties(string expression, ExpressionContext context, ReadingContext readingContext, bool b)
        {
            bool present = false;
            var parser = GetDeriveParser(context, readingContext, null, null);
            parser.SpicePropertyFound += (sender, e) =>
            {
                present = true;
                e.Apply(() => 0);
            };

            parser.Parse(expression);
            return present;
        }

        public static bool HaveFunctions(string expression, ExpressionContext context, ReadingContext readingContext, bool v)
        {
            bool present = false;
            var parser = GetDeriveParser(context, readingContext, null, null);
            parser.FunctionFound += (sender, e) =>
            {
                present = true;
            };

            parser.Parse(expression);
            return present;
        }
    }
}
