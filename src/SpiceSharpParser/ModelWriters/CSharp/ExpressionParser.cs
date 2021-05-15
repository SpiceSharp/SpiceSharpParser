using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class ExpressionParser
    {
        public ExpressionParser(
            RealBuilder doubleBuilder,
            bool throwOnErrors,
            ISpiceNetlistCaseSensitivitySettings caseSettings)
        {
            InternalParser = new Parser();
            DoubleBuilder = doubleBuilder;

            ThrowOnErrors = throwOnErrors;
            CaseSettings = caseSettings;
        }


        public bool ThrowOnErrors { get; }

        public ISpiceNetlistCaseSensitivitySettings CaseSettings { get; }

        protected RealBuilder DoubleBuilder { get; }

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

        public IEnumerable<Node> GetVariables(string expression)
        {
            var node = InternalParser.Parse(expression);
            return GetVariables(node);
        }

        public IEnumerable<Node> GetVariables(Node node)
        {
            var list = new List<Node>();

            DoubleBuilder.VariableFound += (o, e) =>
            {
                list.Add(e.Node);
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
    }
}