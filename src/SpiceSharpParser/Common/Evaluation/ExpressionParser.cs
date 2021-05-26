using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Lexers.Expressions;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.Common
{
    public class ExpressionParser
    {
        public ExpressionParser(
            RealBuilder doubleBuilder,
            bool throwOnErrors)
        {
            DoubleBuilder = doubleBuilder;
            ThrowOnErrors = throwOnErrors;
        }

        public bool ThrowOnErrors { get; }

        protected RealBuilder DoubleBuilder { get; }

        public IEnumerable<string> GetFunctions(string expression)
        {
            var list = new List<string>();
            var node = Parser.Parse(Lexer.FromString(expression));
            DoubleBuilder.FunctionFound += (_, e) =>
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
            var node = Parser.Parse(Lexer.FromString(expression));
            return GetVariables(node);
        }

        public IEnumerable<Node> GetVariables(Node node)
        {
            var list = new List<Node>();

            DoubleBuilder.VariableFound += (_, e) =>
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

        public double Evaluate(string expression)
        {
            var node = Parser.Parse(Lexer.FromString(expression));
            return DoubleBuilder.Build(node);
        }
    }
}