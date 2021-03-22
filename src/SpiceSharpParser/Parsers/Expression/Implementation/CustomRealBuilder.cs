using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpParser.Common.Evaluation;
using System.Collections.Concurrent;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Builders;
using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Parsers.Expression.Implementation
{
    public class CustomRealBuilder : RealBuilder
    {
        public EvaluationContext Context { get; }
        public Parser Parser { get; }

        private readonly ConcurrentDictionary<string, Export> _exporterInstances = new ConcurrentDictionary<string, Export>();
        private readonly ISpiceNetlistCaseSensitivitySettings _caseSettings;
        private List<CustomVariable<Func<double>>> variables = new List<Common.Evaluation.CustomVariable<Func<double>>>();
        private Dictionary<string, List<Node>> der = new Dictionary<string, List<Node>>();


        public CustomRealBuilder(EvaluationContext context, SpiceSharpBehavioral.Parsers.Parser parser, ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            _caseSettings = caseSettings;

            FunctionFound += OnFunctionFound;
            FunctionFound += DoubleBuilder_FunctionFound;
            VariableFound += DoubleBuilder_VariableFound;
            Context = context;
            Parser = parser;


            // setup varaibles
            foreach (var variable in Context.Arguments)
            {
                var variableNode = VariableNode.Variable(variable.Key);
                variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => this.Build(Parser.Parse(variable.Value.ValueExpression)) });
            }

            foreach (var variable in context.Parameters)
            {
                var variableNode = VariableNode.Variable(variable.Key);

                if (variable.Value is ConstantExpression ce)
                {
                    variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => ce.Value });
                }
                else
                {
                    variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => Build(Parser.Parse(variable.Value.ValueExpression)) });
                }
            };

            foreach (var functionName in Context.FunctionsBody.Keys)
            {
                var body = Context.FunctionsBody[functionName];
                if (body != null)
                {
                    var function = Parser.Parse(body);
                    // Derive the function
                    var derivatives = new Derivatives()
                    {
                        Variables = new HashSet<VariableNode>()
                    };

                    var nf = new NodeFinder();
                    foreach (var variable in nf.Build(function).Where(v => v.NodeType == NodeTypes.Voltage || v.NodeType == NodeTypes.Current))
                    {
                        if (derivatives.Variables.Contains(variable))
                            continue;
                        derivatives.Variables.Add(variable);
                    }

                    if (Context.FunctionArguments.ContainsKey(functionName))
                    {
                        var arguments = Context.FunctionArguments[functionName];

                        if (arguments != null)
                        {
                            foreach (var argument in arguments)
                            {
                                derivatives.Variables.Add(VariableNode.Variable(argument));
                            }
                        }
                    }

                    var res = derivatives.Derive(function);

                    int i = 0;
                    foreach (var key in res.Keys)
                    {
                        var derivativeName = "d" + functionName + "(" + i + ")";
                        var derivativeBody = res[key];

                        if (!der.ContainsKey(derivativeName))
                        {
                            der[derivativeName] = new List<Node>();
                        }

                        der[derivativeName].Add(derivativeBody);

                    }
                }
            }
        }

        private static void OnFunctionFound(object sender, FunctionFoundEventArgs<double> args)
        {
            if (!args.Created && RealBuilderHelper.Defaults.TryGetValue(args.Function.Name, out var definition))
            {
                var arguments = new double[args.Function.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                    arguments[i] = args.Builder.Build(args.Function.Arguments[i]);
                args.Result = definition(arguments);
            }
        }

        protected override double BuildNode(Node node)
        {
            if (node is PropertyNode propertyNode)
            {
                var modelName = propertyNode.Name;
                var propertyName = propertyNode.PropertyName;
                string key = $"{Context.Name}_@_{modelName}_{propertyName}";


                if (_exporterInstances.TryGetValue(key, out Export cachedExport))
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
                    var export = propertyExporter.CreateExport(key,
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


        private void DoubleBuilder_VariableFound(object sender, SpiceSharpBehavioral.Builders.Direct.VariableFoundEventArgs<double> e)
        {

            var found = variables.SingleOrDefault(variable => StringComparerProvider.Get(_caseSettings.IsParameterNameCaseSensitive).Equals(variable.Name, e.Node.Name));
            if (found != null)
            {
                e.Result = found.Value();
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

                            var export = currentExporter.CreateExport(key,
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

                        var parameters = new ParameterCollection();
                        var vectorParameter = new VectorParameter();
                        vectorParameter.Elements.Add(new IdentifierParameter(e.Node.Name.ToString()));
                        parameters.Add(vectorParameter);

                        string key = $"{Context.Name}_V_{parameters}_{Context.Simulation?.Name}";

                        if (_exporterInstances.TryGetValue(key, out Export cachedExport))
                        {
                            e.Result = cachedExport.Extract();
                        }
                        else
                        {

                            var currentExporter = new VoltageExporter();

                            var export = currentExporter.CreateExport(key,
                                   "V",
                                   parameters,
                                   Context,
                                   _caseSettings);

                            _exporterInstances[key] = export;

                            e.Result = export.Extract();
                        }
                    }
                }
            }
        }


        private void DoubleBuilder_FunctionFound(object sender, SpiceSharpBehavioral.Builders.Direct.FunctionFoundEventArgs<double> e)
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
            else
            {
                if (der.ContainsKey(name))
                {
                    var arguments = new double[functionArguments.Count];
                    for (var i = 0; i < functionArguments.Count; i++)
                    {
                        arguments[i] = Build(functionArguments[i]);

                        var argNameNode = functionArguments[i];

                        var nf = new NodeFinder();
                        var arg = nf.Build(argNameNode);

                        variables.Add(new CustomVariable<Func<double>>() { Name = arg.First().Name, Value = () => arguments[i] });
                    }
                }
            }

            return null;
        }
    }
}
