using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
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
    public class ExpressionParser
    {

        private readonly ConcurrentDictionary<string, Export> _exporterInstances = new ConcurrentDictionary<string, Export>();
        private readonly ISpiceNetlistCaseSensitivitySettings _caseSettings;
        private List<CustomVariable<Func<double>>> variables = new List<Common.Evaluation.CustomVariable<Func<double>>>();

        private Dictionary<string, List<Node>> der = new Dictionary<string, List<Node>>();

        public Dictionary<string, string> FunctionsBody { get; protected set; }

        public ExpressionParser(EvaluationContext context, bool throwOnErrors, ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            _caseSettings = caseSettings;
            DoubleBuilder = new RealBuilder();

            DoubleBuilder.FunctionFound += DoubleBuilder_FunctionFound;
            DoubleBuilder.VariableFound += DoubleBuilder_VariableFound;
            DoubleBuilder.RegisterDefaultFunctions();

            Context = context;
            ThrowOnErrors = throwOnErrors;

            // setup varaibles
            foreach (var variable in Context.Arguments)
            {
                var variableNode = VariableNode.Variable(variable.Key);
                variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => DoubleBuilder.Build(InternalParser.Parse(variable.Value.ValueExpression)) });
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
                    variables.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => DoubleBuilder.Build(InternalParser.Parse(variable.Value.ValueExpression)) });
                }
            };

            InternalParser = new Parser();

            foreach (var functionName in Context.FunctionsBody.Keys)
            {
                var body = Context.FunctionsBody[functionName];
                if (body != null)
                {
                    var function = InternalParser.Parse(body);
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
                    var arguments = new double[function.ArgumentsCount];
                    for (var i = 0; i < function.ArgumentsCount; i++)
                    {
                        arguments[i] = DoubleBuilder.Build(functionArguments[i]);
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
                        arguments[i] = DoubleBuilder.Build(functionArguments[i]);

                        var argNameNode = functionArguments[i];

                        var nf = new NodeFinder();
                        var arg = nf.Build(argNameNode);
    
                        variables.Add(new CustomVariable<Func<double>>() { Name = arg.First().Name, Value = () => arguments[i] });
                    }


                }
            }

            return null;
        }
    

        protected RealBuilder DoubleBuilder { get; }

        protected Parser InternalParser { get; }
        public EvaluationContext Context { get; }
        public bool ThrowOnErrors { get; }

        public IEnumerable<string> GetFunctions(string expression)
        {
            var list = new List<string>();
            var node = InternalParser.Parse(expression);
            DoubleBuilder.FunctionFound += (o, e) => { list.Add(e.Function.Name); e.Result = 0; };
            DoubleBuilder.Build(node);
            return list;
        }

        public IEnumerable<string> GetVariables(string expression)
        {
            var list = new List<string>();
            var node = InternalParser.Parse(expression);
            DoubleBuilder.VariableFound += (o, e) => { list.Add(e.Node.ToString()); e.Result = 0; };
            DoubleBuilder.Build(node);
            return list;
        }

        public double Parse(string expression)
        {
            var node = InternalParser.Parse(expression);
            return DoubleBuilder.Build(node);
        }
    }
}