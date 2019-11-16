using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
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
            var parser = GetDeriveParser(context, readingContext, evaluator, simulation, readingContext?.CaseSensitivity, @throw);
            var derivatives = parser.Parse(expression);
            return derivatives[0]();
        }

        public static List<string> GetExpressionParameters(string expression, ExpressionContext context, IReadingContext readingContext, SpiceNetlistCaseSensitivitySettings caseSettings, bool @throw)
        {
            var parser = GetDeriveParser(context, readingContext, null, null, caseSettings, @throw);
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

        public static ExpressionParser GetDeriveParser(ExpressionContext expressionContext, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation, SpiceNetlistCaseSensitivitySettings caseSettings,  bool @throw = true)
        {
            var parser = new ExpressionParser(caseSettings);
            
            parser.VariableFound += OnVariableFound(expressionContext, readingContext, evaluator, simulation, @throw);
            parser.FunctionFound += OnFunctionFound(expressionContext, readingContext, evaluator, simulation);
            parser.SpicePropertyFound += OnSpicePropertyFound(expressionContext, readingContext, evaluator, simulation);
            
            return parser;
        }
      
        public static bool HaveSpiceProperties(string expression, ExpressionContext context, ReadingContext readingContext, bool b)
        {
            bool present = false;
            var parser = GetDeriveParser(context, readingContext, null, null, readingContext.CaseSensitivity);
            parser.SpicePropertyFound += (sender, e) =>
            {
                present = true;
                e.Apply(() => 0);
            };

            parser.Parse(expression);
            return present;
        }

        public static bool HaveFunctions(string expression, ExpressionContext context, ReadingContext readingContext)
        {
            bool present = false;
            var parser = GetDeriveParser(context, readingContext, null, null, readingContext.CaseSensitivity);
            parser.FunctionFound += (sender, e) =>
            {
                present = true;
            };

            parser.Parse(expression);
            return present;
        }

        private static EventHandler<SpicePropertyFoundEventArgs<double>> OnSpicePropertyFound(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation)
        {
            return (sender, arg) =>
            {
                if (arg is SimpleDerivativePropertyEventArgs argTyped)
                {
                    if (argTyped.Result != null)
                    {
                        return;
                    }
                }

                if (simulation == null)
                {
                    arg.Apply(() => 0);
                    return;
                }

                ApplySpiceProperty(context, readingContext, evaluator, simulation, arg);
            };
        }

        private static void ApplySpiceProperty(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation, SpicePropertyFoundEventArgs<double> arg)
        {
            var parameters = GetSpicePropertyParameters(context, readingContext, evaluator, simulation, arg);

            var propertyName = arg.Property.Identifier.ToLower();

            string key = $"{propertyName}_{parameters}_{simulation.Name}";

            if (readingContext.ExporterInstances.TryGetValue(key, out Export cachedExport))
            {
                arg.Apply(() => cachedExport.Extract());
            }
            else
            {
                var voltageExportFactory = new VoltageExporter();
                var currentExportFactory = new CurrentExporter();
                var propertyExporter = new PropertyExporter();
                Exporter factory = null;

                if (currentExportFactory.CreatedTypes.Contains(propertyName))
                {
                    factory = currentExportFactory;
                }

                if (voltageExportFactory.CreatedTypes.Contains(propertyName))
                {
                    factory = voltageExportFactory;
                }

                if (propertyName == "@")
                {
                    factory = propertyExporter;
                }

                if (factory == null)
                {
                    throw new Exception("Unknown spice property");
                }

                var export = factory.CreateExport(
                    string.Empty,
                    propertyName,
                    parameters,
                    simulation,
                    readingContext.NodeNameGenerator,
                    readingContext.ComponentNameGenerator,
                    readingContext.ModelNameGenerator,
                    readingContext.Result,
                    readingContext.CaseSensitivity);

                arg.Apply(() => export.Extract());

                readingContext.ExporterInstances[key] = export;
            }
        }

        private static ParameterCollection GetSpicePropertyParameters(ExpressionContext context, IReadingContext readingContext,
            IEvaluator evaluator, Simulation simulation, SpicePropertyFoundEventArgs<double> arg)
        {
            var vectorParameter = new VectorParameter();
            for (var i = 0; i < arg.Property.ArgumentCount; i++)
            {
                var argumentName = arg.Property[i];

                if (context.Arguments.ContainsKey(argumentName))
                {
                    var expression = context.Arguments[argumentName];
                    var argumentValue = evaluator.Evaluate(expression, context, simulation, readingContext);
                    vectorParameter.Elements.Add(new ValueParameter(((int) argumentValue).ToString()));
                }
                else if (context.Parameters.ContainsKey(argumentName))
                {
                    var expression = context.Parameters[argumentName];
                    var argumentValue = evaluator.Evaluate(expression, context, simulation, readingContext);
                    vectorParameter.Elements.Add(new ValueParameter(((int) argumentValue).ToString()));
                }
                else
                {
                    vectorParameter.Elements.Add(new WordParameter(argumentName));
                }
            }

            var parameters = new ParameterCollection { vectorParameter };
            return parameters;
        }

        private static EventHandler<VariableFoundEventArgs<Derivatives<Func<double>>>> OnVariableFound(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation, bool @throw)
        {
            return (sender, args) =>
            {
                if (context.Arguments.Any(a => a.Key == args.Name))
                {
                    var d = new DoubleDerivatives(2)
                    {
                        [0] = () =>
                        {
                            var expression = context.Arguments.First(a => a.Key == args.Name).Value;
                            var value = evaluator.Evaluate(expression, context, simulation, readingContext);
                            return value;
                        },
                        [1] = () => 1
                    };
                    args.Result = d;
                    return;
                }

                if (context.Parameters.ContainsKey(args.Name))
                {
                    var d = new DoubleDerivatives(2)
                    {
                        [0] = () =>
                        {
                            var parameter = context.Parameters[args.Name];
                            var value = evaluator.Evaluate(parameter, context, simulation, readingContext);
                            return value;
                        },
                        [1] = () => 0
                    };
                    args.Result = d;
                    return;
                }

                if (readingContext != null)
                {
                    var readingExpressionContext = readingContext.ReadingExpressionContext;

                    if (readingExpressionContext.Parameters.ContainsKey(args.Name))
                    {
                        var d = new DoubleDerivatives(2)
                        {
                            [0] = () =>
                            {
                                var expression = readingExpressionContext.Parameters[args.Name];
                                var value = evaluator.Evaluate(expression, context, simulation, readingContext);
                                return value;
                            },
                            [1] = () => 0
                        };
                        args.Result = d;
                        return;
                    }
                }

                if (@throw)
                {
                    throw new UnknownParameterException(args.Name);
                }
                else
                {
                    var d = new DoubleDerivatives(1) { [0] = () => double.NaN };
                    args.Result = d;
                }
            };
        }

        private static EventHandler<FunctionFoundEventArgs<Derivatives<Func<double>>>> OnFunctionFound(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation)
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
                        SetResult(context, readingContext, evaluator, simulation, args, doubleFunction);
                        return;
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
                            SetResult(context, readingContext, evaluator, simulation, args, doubleFunction);
                        }
                    }
                }
            };
        }

        private static void SetResult(ExpressionContext context, IReadingContext readingContext, IEvaluator evaluator, Simulation simulation, FunctionFoundEventArgs<Derivatives<Func<double>>> args, IFunction<double, double> doubleFunction)
        {
            var argumentCount = args.ArgumentCount;

            var variableCount = 1;
            for (var i = 0; i < argumentCount; i++)
            {
                variableCount = Math.Max(variableCount, args[i].Count);
            }

            var result = new DoubleDerivatives(variableCount);

            var arguments = new Func<double>[argumentCount];
            for (var i = 0; i < argumentCount; i++)
            {
                var iLocal = i;
                arguments[i] = () => args[iLocal][0]();
            }

            result[0] = () =>
            {
                var argumentValues = arguments.Select(arg => arg()).ToArray();

                return doubleFunction.Logic(
                    string.Empty,
                    argumentValues,
                    evaluator ?? readingContext?.ReadingEvaluator,
                    context,
                    simulation,
                    readingContext);
            };

            if (doubleFunction is IDerivativeFunction<double, double> derivativeFunction)
            {
                var derivatives = new Lazy<Derivatives<Func<double>>>(() =>
                    derivativeFunction.Derivative(
                        string.Empty,
                        arguments,
                        evaluator ?? readingContext?.ReadingEvaluator,
                        context,
                        simulation,
                        readingContext));

                for (var i = 1; i <= variableCount; i++)
                {
                    var iLocal = i;

                    if (IsDerivativeDefined(args, iLocal))
                    {
                        result[i] = () => GetDerivativeValue(args, iLocal, argumentCount, derivatives);
                    }
                }
            }

            args.Result = result;
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

        private static double GetDerivativeValue(FunctionFoundEventArgs<Derivatives<Func<double>>> args, int derivativeIndex, int argumentCount, Lazy<Derivatives<Func<double>>> derivatives)
        {
            var result = 0.0;

            for (var argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
            {
                if (args[argumentIndex][derivativeIndex] != null)
                {
                    var derivative = derivatives.Value[argumentIndex + 1];

                    if (derivative != null)
                    {
                        result += args[argumentIndex][derivativeIndex]() * derivative.Invoke();
                    }
                }
            }

            return result;
        }

    }
}
