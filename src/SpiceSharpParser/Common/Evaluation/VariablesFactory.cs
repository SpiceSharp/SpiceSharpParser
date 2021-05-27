using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.Parsers.Expression;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common.Evaluation
{
    public class VariablesFactory
    {
        public List<CustomVariable<Func<double>>> CreateVariables(IEvaluationContext context,  RealBuilder builder)
        {
            var result = new List<CustomVariable<Func<double>>>();

            // setup variables
            foreach (var variable in context.Arguments)
            {
                var variableNode = Parser.Parse(Lexer.FromString(variable.Value.ValueExpression));

                result.Add(
                    new CustomVariable<Func<double>>() { Name = variable.Key, VariableNode = variableNode, Value = () => builder.Build(variableNode) });
            }

            foreach (var variable in context.Parameters)
            {
                if (variable.Value is ConstantExpression ce)
                {
                    result.Add(new CustomVariable<Func<double>>() { Name = variable.Key, Value = () => ce.Value, Constant = true });
                }
                else
                {
                    var variableNode = Parser.Parse(Lexer.FromString(variable.Value.ValueExpression));
                    result.Add(new CustomVariable<Func<double>>() { Name = variable.Key, VariableNode = variableNode, Value = () => builder.Build(variableNode) });
                }
            }

            return result;
        }
    }
}
