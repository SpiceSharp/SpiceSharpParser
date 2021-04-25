using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.Parsers.Expression.Implementation;
using SpiceSharpParser.Parsers.Expression.Implementation.ResolverFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using Parser = SpiceSharpParser.Parsers.Expression.Implementation.Parser;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParser
    {
        public ExpressionParser(EvaluationContext context, bool throwOnErrors, ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            InternalParser = new Parser();
            DoubleBuilder = new CustomRealBuilder(context, InternalParser, caseSettings, throwOnErrors);

            Context = context;
            ThrowOnErrors = throwOnErrors;
            CaseSettings = caseSettings;
        }

        public EvaluationContext Context { get; }

        public bool ThrowOnErrors { get; }

        public ISpiceNetlistCaseSensitivitySettings CaseSettings { get; }
        
        protected CustomRealBuilder DoubleBuilder { get; }

        protected Parser InternalParser { get; }

        public IEnumerable<string> GetFunctions(string expression)
        {
            var list = new List<string>();
            var node = InternalParser.Parse(expression);
            DoubleBuilder.FunctionFound += (o, e) =>
            {
                list.Add(e.Function.Name);
                e.Result = 0;
            };

            try
            {
                DoubleBuilder.Build(node);
            }
            catch (Exception)
            {
                if (ThrowOnErrors)
                {
                    throw;
                }
            }

            return list;
        }

        public IEnumerable<string> GetVariables(string expression)
        {
            var list = new List<string>();
            var node = InternalParser.Parse(expression);
            DoubleBuilder.VariableFound += (o, e) =>
            {
                list.Add(e.Node.ToString());
                e.Result = 0;
            };
            try
            {
                DoubleBuilder.Build(node);
            }
            catch (Exception)
            {
                if (ThrowOnErrors)
                {
                    throw;
                }
            }

            return list;
        }

        public double Parse(string expression)
        {
            var node = InternalParser.Parse(expression);
            return DoubleBuilder.Build(node);
        }

        public Node Resolve(string expression)
        {
            var node = InternalParser.Parse(expression);
            try
            {
                var resolver = new Resolver();
                var comparer = StringComparerProvider.Get(CaseSettings.IsParameterNameCaseSensitive);

                if (Context.NameGenerator.NodeNameGenerator is SubcircuitNodeNameGenerator)
                {
                    resolver.UnknownVariableFound += (sender, args) =>
                    {
                        if (args.Node.NodeType == NodeTypes.Voltage)
                        {
                            if (resolver.VariableMap.Any(v => comparer.Equals(v.Key, args.Node.Name)))
                            {
                                var node = resolver.VariableMap.First(v => comparer.Equals(v.Key, args.Node.Name)).Value;
                                args.Result = VariableNode.Voltage(node.ToString());
                            }
                            else
                            {
                                args.Result = VariableNode.Voltage(Context.NameGenerator.ParseNodeName(args.Node.Name));
                            }
                        }

                        if (args.Node.NodeType == NodeTypes.Current)
                        {
                            args.Result = VariableNode.Current(Context.NameGenerator.GenerateObjectName(args.Node.Name));
                        }
                    };
                }
                else
                {
                    resolver.UnknownVariableFound += (sender, args) =>
                    {
                        if (args.Node.NodeType == NodeTypes.Voltage)
                        {
                            if (resolver.VariableMap.Any(v => comparer.Equals(v.Key, args.Node.Name)))
                            {
                                var node = resolver.VariableMap.First(v => comparer.Equals(v.Key, args.Node.Name)).Value;
                                args.Result = VariableNode.Voltage(node.ToString());
                            }
                            else
                            {
                                args.Result = VariableNode.Voltage(args.Node.Name);
                            }
                        }

                        if (args.Node.NodeType == NodeTypes.Current)
                        {
                            args.Result = VariableNode.Current(args.Node.Name);
                        }
                    };
                }

                resolver.FunctionMap = CreateFunctions();
                resolver.VariableMap = new Dictionary<string, Node>(StringComparerProvider.Get(CaseSettings.IsParameterNameCaseSensitive));
               
                foreach (var variable in DoubleBuilder.Variables)
                {
                    if (variable.Constant)
                    {
                        resolver.VariableMap[variable.Name] = variable.Value();
                    }
                    else
                    {
                        resolver.VariableMap[variable.Name] = variable.VariableNode;
                    }
                }

                // TIME variable is handled well in SpiceSharpBehavioral
                resolver.VariableMap.Remove("TIME");

                var resolved = resolver.Resolve(node);
                return resolved;
            }
            catch (Exception)
            {
                if (ThrowOnErrors)
                {
                    throw;
                }
            }

            return node;
        }

        public Dictionary<string, ResolverFunction> CreateFunctions()
        {
            var result = new Dictionary<string, ResolverFunction>(StringComparerProvider.Get(Context.CaseSettings.IsFunctionNameCaseSensitive));

            foreach (var functionName in Context.FunctionsBody.Keys)
            {
                var body = Context.FunctionsBody[functionName];
                if (body != null)
                {
                    var bodyNode = InternalParser.Parse(body);

                    result[functionName] = new StaticResolverFunction(
                        functionName,
                        bodyNode,
                        Context.FunctionArguments[functionName].Select(a => Node.Variable(a)).ToList());
                }
            }

            result["poly"] = new PolyResolverFunction();
            result["if"] = new IfResolverFunction();
            result["max"] = new MaxResolverFunction();
            result["random"] = new RandomResolverFunction(Context);
            result["gauss"] = new GaussResolverFunction(Context);

            return result;
        }
    }
}