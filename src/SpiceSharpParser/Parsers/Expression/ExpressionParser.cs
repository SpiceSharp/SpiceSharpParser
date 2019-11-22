using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParser : IExpressionParser
    {
        private readonly ISpiceNetlistCaseSensitivitySettings _caseSettings;
        private readonly ConcurrentDictionary<string, Export> _exporterInstances = new ConcurrentDictionary<string, Export>();

        public ExpressionParser(ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            _caseSettings = caseSettings;
        }

        public double GetExpressionValue(string expression, EvaluationContext context, bool @throw = true)
        {
            var parser = GetDeriveParser(context, @throw);
            var derivatives = parser.Parse(expression);
            return derivatives[0]();
        }

        public List<string> GetExpressionParameters(string expression, EvaluationContext context, bool @throw = true)
        {
            var parser = GetDeriveParser(context, @throw);
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

        public SimpleDerivativeParser GetDeriveParser(EvaluationContext context, bool @throw = true)
        {
            var parser = new SimpleDerivativeParserExtended(_caseSettings);

            parser.VariableFound += OnVariableFound(context, @throw);
            parser.FunctionFound += OnFunctionFound(context);
            parser.SpicePropertyFound += OnSpicePropertyFound(context);

            return parser;
        }

        public bool HaveSpiceProperties(string expression, EvaluationContext context)
        {
            bool present = false;
            var parser = GetDeriveParser(context);
            parser.SpicePropertyFound += (sender, e) =>
            {
                present = true;
                e.Apply(() => 0);
            };

            parser.Parse(expression);
            return present;
        }

        public bool HaveFunctions(string expression, EvaluationContext context)
        {
            bool present = false;
            var parser = GetDeriveParser(context);
            parser.FunctionFound += (sender, e) =>
            {
                present = true;
            };

            parser.Parse(expression);
            return present;
        }

        private EventHandler<SpicePropertyFoundEventArgs<double>> OnSpicePropertyFound(EvaluationContext context)
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

                if (context.Simulation == null)
                {
                    arg.Apply(() => 0);
                    return;
                }

                ApplySpiceProperty(context, arg);
            };
        }

        private void ApplySpiceProperty(EvaluationContext context, SpicePropertyFoundEventArgs<double> arg)
        {
            var parameters = GetSpicePropertyParameters(context, arg);

            var propertyName = arg.Property.Identifier.ToLower();

            string key = $"{context.Name}_{propertyName}_{parameters}_{context.Simulation?.Name}";

            if (_exporterInstances.TryGetValue(key, out Export cachedExport))
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
                    propertyName + "_" + parameters.ToString(),
                    propertyName,
                    parameters,
                    context,
                    _caseSettings);

                _exporterInstances[key] = export;

                arg.Apply(() => export.Extract());
            }
        }

        private ParameterCollection GetSpicePropertyParameters(EvaluationContext context, SpicePropertyFoundEventArgs<double> arg)
        {
            var vectorParameter = new VectorParameter();
            for (var i = 0; i < arg.Property.ArgumentCount; i++)
            {
                var argumentName = arg.Property[i];
                if (context.Parameters.ContainsKey(argumentName))
                {
                    var val = context.Evaluate(argumentName);
                    vectorParameter.Elements.Add(new WordParameter(val.ToString()));
                }
                else
                {
                    vectorParameter.Elements.Add(new WordParameter(argumentName));
                }
            }

            var parameters = new ParameterCollection { vectorParameter };
            return parameters;
        }

        private EventHandler<VariableFoundEventArgs<Derivatives<Func<double>>>> OnVariableFound(EvaluationContext context, bool @throw)
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
                            var value = context.Evaluate(expression);
                            return value;
                        },
                        [1] = () => 1,
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
                            var value = context.Evaluate(parameter);
                            return value;
                        },
                        [1] = () => 0,
                    };
                    args.Result = d;
                    return;
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

        private EventHandler<FunctionFoundEventArgs<Derivatives<Func<double>>>> OnFunctionFound(EvaluationContext context)
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
                        SetResult(context, args, doubleFunction);
                    }
                }
            };
        }

        private void SetResult(EvaluationContext context, FunctionFoundEventArgs<Derivatives<Func<double>>> args, IFunction<double, double> doubleFunction)
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

                return doubleFunction.Logic(string.Empty, argumentValues, context);
            };

            if (doubleFunction is IDerivativeFunction<double, double> derivativeFunction)
            {
                var derivatives = new Lazy<Derivatives<Func<double>>>(() =>
                    derivativeFunction.Derivative(string.Empty, arguments, context));

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