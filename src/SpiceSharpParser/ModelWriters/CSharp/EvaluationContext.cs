using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class EvaluationContext : IEvaluationContext
    {
        public EvaluationContext(ExpressionParser parser)
        {
            Parser = parser;
        }

        public ExpressionParser Parser { get; }

        public Dictionary<string, Expression> Parameters { get; set; } = new Dictionary<string, Expression>();

        public Dictionary<string, Expression> Arguments { get; set; } = new Dictionary<string, Expression>();

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public List<string> Functions { get; set; } = new List<string>();

        public IEvaluator Evaluator { get; set; }

        public bool HaveSpiceProperties(string expression)
        {
            var variables = Parser.GetVariables(expression).ToList();

            return
                variables.Any(v => v.NodeType == SpiceSharpBehavioral.Parsers.Nodes.NodeTypes.Current)
                || variables.Any(v => v.NodeType == SpiceSharpBehavioral.Parsers.Nodes.NodeTypes.Voltage);
        }

        public string Transform(string expression)
        {
            var node = Parsers.Expression.Parser.Parse(Lexer.FromString(expression));
            var transformer = new ExpressionTransformer(Variables.Select(v => v.Key).ToList(), Functions);
            var parameterFunction = transformer.Transform(node);
            return parameterFunction;
        }

        public double Evaluate(Parameter something)
        {
            return Parser.Evaluate(something.Value);
        }

        public double Evaluate(string expression)
        {
            return Parser.Evaluate(expression);
        }

        public bool HaveFunctions(string expression)
        {
            var functions = Parser.GetFunctions(expression);
            return functions.Any();
        }

        public bool HaveFunction(string expression, string functionName)
        {
            var functions = Parser.GetFunctions(expression);
            return functions.Any(function => function.ToLower() == functionName.ToLower());
        }

        public bool HaveVariables(string expression)
        {
            var variables = Parser.GetVariables(expression);
            return variables.Any();
        }
    }
}
