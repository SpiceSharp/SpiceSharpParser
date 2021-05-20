using SpiceSharpBehavioral.Parsers.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class ExpressionTransformer
    {
        public ExpressionTransformer(List<string> variables, List<string> functions)
        {
            Variables = variables;
            Functions = functions;
        }

        public List<string> Variables { get; }

        public List<string> Functions { get; }

        public string Transform(Node expression)
        {
            switch (expression)
            {
                case UnaryOperatorNode un:
                    switch (un.NodeType)
                    {
                        case NodeTypes.Plus: return $"+({Transform(un.Argument)})";
                        case NodeTypes.Minus: return $"-({Transform(un.Argument)})";
                        case NodeTypes.Not: return $"!({Transform(un.Argument)})";
                    }

                    break;

                case BinaryOperatorNode bn:
                    var left = Transform(bn.Left);
                    var right = Transform(bn.Right);
                    switch (bn.NodeType)
                    {
                        case NodeTypes.Add: return $"({left}) + ({right})";
                        case NodeTypes.Subtract: return $"({left}) - ({right})";
                        case NodeTypes.Multiply: return $"({left}) * ({right})";
                        case NodeTypes.Divide: return $"({left})/ ({right})";
                        case NodeTypes.Modulo: return $"({left}) % ({right})";
                        case NodeTypes.LessThan: return $"({left}) < ({right})";
                        case NodeTypes.GreaterThan: return $"({left}) > ({right})";
                        case NodeTypes.LessThanOrEqual: return $"({left}) <= ({right})";
                        case NodeTypes.GreaterThanOrEqual: return $"({left}) >= ({right})";
                        case NodeTypes.Equals: return $"({left}) == ({right})";
                        case NodeTypes.NotEquals: return $"({left}) != ({right})";
                        case NodeTypes.And: return $"({left}) & ({right})";
                        case NodeTypes.Or: return $"({left})|({right})";
                        case NodeTypes.Xor: return $"({left}) ^ ({right})";
                        case NodeTypes.Pow: return $"({left}) ** ({right})";
                    }

                    break;

                case TernaryOperatorNode tn:
                    return $"({Transform(tn.Condition)}) ? ({Transform(tn.IfTrue)}) : ({Transform(tn.IfFalse)})";

                case FunctionNode fn:
                    if (Functions.Contains(fn.Name))
                    {
                        return @$"{{{fn.Name}($""{string.Join(",", fn.Arguments.Select(@arg => Transform(@arg)).ToList().AsReadOnly())}"")}}";
                    }
                    else
                    {
                        return $"{fn.Name}({string.Join(",", fn.Arguments.Select(@arg => Transform(@arg)).ToList().AsReadOnly())})";
                    }

                case ConstantNode cn:
                    return $"{{{cn}}}";

                case VariableNode vn:
                    if (vn.NodeType == NodeTypes.Variable)
                    {
                        if (Variables.Contains(vn.Name))
                        {
                            return $"{{{vn.Name}}}";
                        }
                        else
                        {
                            return $"{{{vn.Name}()}}";
                        }
                    }

                    if (vn.NodeType == NodeTypes.Voltage || vn.NodeType == NodeTypes.Current)
                    {
                        return vn.ToString();
                    }

                    break;
            }

            return expression.ToString();
        }
    }
}
