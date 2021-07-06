using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.ResolverFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation
{
    public class ExpressionResolver
    {
        public ExpressionResolver(
            RealBuilder doubleBuilder,
            EvaluationContext context,
            bool throwOnErrors,
            SpiceNetlistCaseSensitivitySettings caseSettings,
            VariablesFactory variablesFactory = null)
        {
            DoubleBuilder = doubleBuilder;

            Context = context;
            ThrowOnErrors = throwOnErrors;
            CaseSettings = caseSettings;
            VariablesFactory = variablesFactory;
        }

        public EvaluationContext Context { get; }

        public bool ThrowOnErrors { get; }

        public SpiceNetlistCaseSensitivitySettings CaseSettings { get; }

        public VariablesFactory VariablesFactory { get; }

        protected RealBuilder DoubleBuilder { get; }

        public Node Resolve(string expression)
        {
            var node = Parser.Parse(Lexer.FromString(expression));
            try
            {
                var resolver = new Resolver();
                var comparer = StringComparerProvider.Get(CaseSettings.IsParameterNameCaseSensitive);

                resolver.UnknownVariableFound += (_, args) =>
                {
                    if (args.Node.NodeType == NodeTypes.Voltage)
                    {
                        if (resolver.VariableMap.Any(v => comparer.Equals(v.Key, args.Node.Name)))
                        {
                            var variableNode = resolver.VariableMap.First(v => comparer.Equals(v.Key, args.Node.Name)).Value;
                            args.Result = VariableNode.Voltage(variableNode.ToString());
                        }
                        else
                        {
                            if (Context.CircuitContext.ReaderSettings.ExpandSubcircuits)
                            {
                                args.Result = VariableNode.Voltage(Context.NameGenerator.ParseNodeName(args.Node.Name));
                            }
                            else
                            {
                                args.Result = args.Node;
                            }
                        }
                    }

                    if (args.Node.NodeType == NodeTypes.Current)
                    {
                        if (Context.CircuitContext.ReaderSettings.ExpandSubcircuits)
                        {
                            args.Result = VariableNode.Current(Context.NameGenerator.GenerateObjectName(args.Node.Name));
                        }
                        else
                        {
                            args.Result = args.Node;
                        }
                    }
                };
               
                resolver.FunctionMap = CreateFunctions();
                resolver.VariableMap = new Dictionary<string, Node>(StringComparerProvider.Get(CaseSettings.IsParameterNameCaseSensitive));

                if (VariablesFactory != null)
                {
                    var variables = VariablesFactory.CreateVariables(Context, DoubleBuilder);

                    foreach (var variable in variables)
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
                    var bodyNode = Parser.Parse(Lexer.FromString(body));

                    result[functionName] = new StaticResolverFunction(
                        functionName,
                        bodyNode,
                        Context.FunctionArguments[functionName].Select(a => Node.Variable(a)).ToList());
                }
            }

            result["poly"] = new PolyResolverFunction();
            result["if"] = new IfResolverFunction();
            result["random"] = new RandomResolverFunction(Context);
            result["gauss"] = new GaussResolverFunction(Context);

            return result;
        }
    }
}