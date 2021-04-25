using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Parsers.Expression.Implementation
{
    public class CustomRealBuilder : RealBuilder
    {
        private readonly ConcurrentDictionary<string, Export> _exporterInstances = new ConcurrentDictionary<string, Export>();

        private readonly ISpiceNetlistCaseSensitivitySettings _caseSettings;

        public CustomRealBuilder(EvaluationContext context, Parser parser, ISpiceNetlistCaseSensitivitySettings caseSettings, bool throwOnErrors)
        {
            _caseSettings = caseSettings;

            FunctionFound += OnDefaultFunctionFound;
            FunctionFound += OnCustomFunctionFound;
            VariableFound += OnVariableFound;
            Context = context;
            Parser = parser;

            // setup variables
            foreach (var variable in Context.Arguments)
            {
                var variableNode = Parser.Parse(variable.Value.ValueExpression);

                Variables.Add(
                    new CustomVariable<Func<double>>() { Name = variable.Key, VariableNode = variableNode, Value = () => this.Build(variableNode) });
            }

            foreach (var variable in context.Parameters)
            {

                if (variable.Value is ConstantExpression ce)
                {
                    Variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => ce.Value, Constant = true });
                }
                else
                {
                    var variableNode = Parser.Parse(variable.Value.ValueExpression);
                    Variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, VariableNode = variableNode, Value = () => Build(variableNode) });
                }
            }

            ThrowOnErrors = throwOnErrors;
        }

        public EvaluationContext Context { get; }

        public Parser Parser { get; }

        public List<CustomVariable<Func<double>>> Variables { get; set; } = new List<Common.Evaluation.CustomVariable<Func<double>>>();
        public bool ThrowOnErrors { get; }

        protected override double BuildNode(Node node)
        {
            if (node is PropertyNode propertyNode)
            {
                var modelName = propertyNode.Name;
                var propertyName = propertyNode.PropertyName;
                string key = $"{Context.Name}_@_{modelName}_{propertyName}";

                if (_exporterInstances.TryGetValue(key, out Export cachedExport) && cachedExport.Simulation != null)
                {
                    return cachedExport.Extract();
                }
                else
                {
                    var propertyExporter = new PropertyExporter();
                    var parameters = new ParameterCollection();
                    var vectorParameter = new VectorParameter();
                    vectorParameter.Elements.Add(new IdentifierParameter(modelName));
                    vectorParameter.Elements.Add(new IdentifierParameter(propertyName));
                    parameters.Add(vectorParameter);
                    var export = propertyExporter.CreateExport(
                        key,
                        "@",
                        parameters,
                        Context,
                        _caseSettings);

                    _exporterInstances[key] = export;

                    return export.Extract();
                }
            }

            return base.BuildNode(node);
        }

        private static void OnDefaultFunctionFound(object sender, FunctionFoundEventArgs<double> args)
        {
            if (!args.Created && RealBuilderHelper.Defaults.TryGetValue(args.Function.Name, out var definition))
            {
                var arguments = new double[args.Function.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    arguments[i] = args.Builder.Build(args.Function.Arguments[i]);
                }

                args.Result = definition(arguments);
            }
        }

        private void OnVariableFound(object sender, SpiceSharpBehavioral.Builders.Direct.VariableFoundEventArgs<double> e)
        {
            if (e.Node.NodeType != NodeTypes.Voltage && e.Node.NodeType != NodeTypes.Current && e.Node.NodeType != NodeTypes.Property)
            {
                var found = Variables.SingleOrDefault(variable => StringComparerProvider.Get(_caseSettings.IsParameterNameCaseSensitive).Equals(variable.Name, e.Node.Name));
                if (found != null)
                {
                    e.Result = found.Value();
                }
            }
            else
            {
                if (Context.Simulation == null)
                {
                    e.Result = 0;
                }
                else
                {
                    if (e.Node.NodeType == NodeTypes.Current)
                    {
                        var name = e.Node.Name;

                        var parameters = new ParameterCollection();
                        var vectorParameter = new VectorParameter();
                        vectorParameter.Elements.Add(new IdentifierParameter(e.Node.Name.ToString()));
                        parameters.Add(vectorParameter);

                        string key = $"{Context.Name}_I_{parameters}_{Context.Simulation?.Name}";

                        if (_exporterInstances.TryGetValue(key, out Export cachedExport))
                        {
                            e.Result = cachedExport.Extract();
                        }
                        else
                        {
                            var currentExporter = new CurrentExporter();

                            var export = currentExporter.CreateExport(
                                key,
                                "I",
                                parameters,
                                Context,
                                _caseSettings);

                            _exporterInstances[key] = export;

                            e.Result = export.Extract();
                        }
                    }
                    else if (e.Node.NodeType == NodeTypes.Voltage)
                    {
                        var name = e.Node.Name;

                        var variable = Variables.FirstOrDefault(v => v.Name == name);
                        if (variable != null)
                        {
                            name = variable.Value().ToString();
                        }

                        var parameters = new ParameterCollection();
                        var vectorParameter = new VectorParameter();
                        vectorParameter.Elements.Add(new IdentifierParameter(name));
                        parameters.Add(vectorParameter);

                        string key = $"{Context.Name}_V_{parameters}_{Context.Simulation?.Name}";

                        if (_exporterInstances.TryGetValue(key, out Export cachedExport))
                        {
                            e.Result = cachedExport.Extract();
                        }
                        else
                        {
                            var currentExporter = new VoltageExporter();

                            var export = currentExporter.CreateExport(
                                key,
                                "V",
                                parameters,
                                Context,
                                _caseSettings);

                            _exporterInstances[key] = export;

                            try
                            {
                                e.Result = export.Extract();
                            }
                            catch (Exception ex)
                            {
                                if (ThrowOnErrors)
                                {
                                    throw;
                                }

                                e.Result = 0;
                            }
                        }
                    }
                }
            }
        }

        private void OnCustomFunctionFound(object sender, SpiceSharpBehavioral.Builders.Direct.FunctionFoundEventArgs<double> e)
        {
            if (!e.Created)
            {
                var result = ComputeFunctionValue(e.Function.Name, e.Function.Arguments);
                if (result != null)
                {
                    e.Result = result.Value;
                }
            }
        }

        private double? ComputeFunctionValue(string name, IReadOnlyList<Node> functionArguments)
        {
            if (Context.Functions.ContainsKey(name))
            {
                var function = Context.Functions[name].First();

                if (function is IFunction<double, double> doubleFunction)
                {
                    var arguments = new double[function.ArgumentsCount >= 0 ? function.ArgumentsCount : functionArguments.Count];
                    for (var i = 0; i < function.ArgumentsCount; i++)
                    {
                        arguments[i] = Build(functionArguments[i]);
                    }

                    var value = doubleFunction.Logic(function.Name, arguments, Context);

                    return value;
                }
            }

            if (Context.FunctionsBody.ContainsKey(name))
            {
                var functionBody = Context.FunctionsBody[name];

                var argumentsDefinition = Context.FunctionArguments[name].ToArray();

                for (var i = 0; i < argumentsDefinition.Length; i++)
                {
                    var argumentValue = Build(Parser.Parse(argumentsDefinition[i]));
                    Context.Arguments.Add(argumentsDefinition[i], new ConstantExpression(argumentValue));
                }

                return Build(Parser.Parse(functionBody));
            }

            return null;
        }
    }
}
