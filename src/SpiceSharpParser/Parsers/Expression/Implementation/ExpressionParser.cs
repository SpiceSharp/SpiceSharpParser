using SpiceSharpBehavioral.Builders;
using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.Parsers.Expression.Implementation;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.Expression
{
    public class ExpressionParser
    {
        public ExpressionParser(EvaluationContext context, bool throwOnErrors, ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            InternalParser = new Parser();
            DoubleBuilder = new CustomRealBuilder(context, InternalParser, caseSettings);

            Context = context;
            ThrowOnErrors = throwOnErrors;
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

            try
            {
                DoubleBuilder.Build(node);
            }
            catch (Exception ex)
            {
                if (ThrowOnErrors) throw;
            }
            return list;
        }

        public IEnumerable<string> GetVariables(string expression)
        {
            var list = new List<string>();
            var node = InternalParser.Parse(expression);
            DoubleBuilder.VariableFound += (o, e) => { list.Add(e.Node.ToString()); e.Result = 0; };
            try
            {
                DoubleBuilder.Build(node);
            }
            catch (Exception ex)
            {
                if (ThrowOnErrors) throw;
            }

            return list;
        }

        public double Parse(string expression)
        {
            var node = InternalParser.Parse(expression);
            return DoubleBuilder.Build(node);
        }

        public Node MakeVariablesGlobal(string expression)
        {
            var node = InternalParser.Parse(expression);

            var builder = new NodeReplacer();
            builder.Map = new Dictionary<VariableNode, Node>();

            if (Context.NameGenerator?.NodeNameGenerator is SubcircuitNodeNameGenerator sng)
            {
                foreach (var pin in sng.PinMap)
                {
                    builder.Map[VariableNode.Voltage(pin.Key)] = VariableNode.Voltage(pin.Value);
                }
            }

            return builder.Build(node);
        }
    }
}