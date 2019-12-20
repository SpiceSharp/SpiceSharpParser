using System;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters.VoltageExports;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParserFactory : IExpressionParserFactory
    {
        private readonly ConcurrentDictionary<string, Export> _exporterInstances = new ConcurrentDictionary<string, Export>();
        private readonly ISpiceNetlistCaseSensitivitySettings _caseSettings;

        public ExpressionParserFactory(ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            _caseSettings = caseSettings;
        }

        public SimpleDerivativeParser Create(EvaluationContext context, bool throwOnErrors = true, bool applyVoltage = false)
        {
            var parser = new SimpleDerivativeParserExtended(_caseSettings);

            parser.VariableFound += OnVariableFound(context, throwOnErrors);
            parser.FunctionFound += OnFunctionFound(context);
            parser.SpicePropertyFound += OnSpicePropertyFound(context, applyVoltage);

            return parser;
        }

        private EventHandler<SpicePropertyFoundEventArgs<double>> OnSpicePropertyFound(EvaluationContext context, bool applyVoltage)
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

                ApplySpiceProperty(context, arg, applyVoltage);
            };
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
                    throw new InvalidParameterException(args.Name, null);
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

        private ParameterCollection GetSpicePropertyParameters(EvaluationContext context, SpicePropertyFoundEventArgs<double> arg)
        {
            var vectorParameter = new VectorParameter(new List<SingleParameter>());
            for (var i = 0; i < arg.Property.ArgumentCount; i++)
            {
                var argumentName = arg.Property[i];
                if (context.Parameters.ContainsKey(argumentName))
                {
                    var val = context.Evaluate(argumentName);
                    vectorParameter.Elements.Add(new WordParameter(val.ToString(CultureInfo.InvariantCulture), null));
                }
                else
                {
                    vectorParameter.Elements.Add(new WordParameter(argumentName, null));
                }
            }

            var parameters = new ParameterCollection(new List<Parameter> { vectorParameter });
            return parameters;
        }

        private void ApplySpiceProperty(EvaluationContext context, SpicePropertyFoundEventArgs<double> arg, bool applyVoltage)
        {
            var parameters = GetSpicePropertyParameters(context, arg);

            var propertyName = arg.Property.Identifier.ToLower();

            string key = $"{context.Name}_{propertyName}_{parameters}_{context.Simulation?.Name}";

            if (_exporterInstances.TryGetValue(key, out Export cachedExport))
            {
                ApplyExport(arg, cachedExport, applyVoltage);
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
                    throw new SpiceSharpParserException($"Unknown spice property {propertyName}");
                }

                var export = factory.CreateExport(
                    $"{propertyName}_{parameters}",
                    propertyName,
                    parameters,
                    context,
                    _caseSettings);

                _exporterInstances[key] = export;

                ApplyExport(arg, export, applyVoltage);
            }
        }

        private void ApplyExport(SpicePropertyFoundEventArgs<double> arg, Export export, bool applyVoltage)
        {
            if (export is VoltageExport)
            {
                if (applyVoltage)
                {
                    arg.Apply(() => export.Extract(), 0, 0);
                }
            }
            else
            {
                arg.Apply(() => export.Extract(), 0, 0);
            }
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
                var derivatives = new Lazy<Derivatives<Func<double>>>(() => derivativeFunction.Derivative(string.Empty, args, context));

                for (var i = 1; i <= variableCount; i++)
                {
                    var derivativeIndex = i;

                    if (IsDerivativeDefined(args, derivativeIndex))
                    {
                        result[i] = () => GetDerivativeValue(args, derivativeIndex, argumentCount, derivatives);
                    }
                }
            }

            args.Result = result;
        }

        private bool IsDerivativeDefined(FunctionFoundEventArgs<Derivatives<Func<double>>> args, int derivativeIndex)
        {
            for (var i = 0; i < args.ArgumentCount; i++)
            {
                if (args[i][derivativeIndex] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private double GetDerivativeValue(FunctionFoundEventArgs<Derivatives<Func<double>>> args, int derivativeIndex, int argumentCount, Lazy<Derivatives<Func<double>>> derivatives)
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