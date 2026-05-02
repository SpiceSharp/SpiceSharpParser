using System.Globalization;
using System.Linq;
using SpiceSharpBehavioral.Parsers.Nodes;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources
{
    internal sealed class BehavioralExpressionFormatter
    {
        public string Format(Node node)
        {
            switch (node)
            {
                case ConstantNode constantNode:
                    return constantNode.Literal.ToString("R", CultureInfo.InvariantCulture);

                case VariableNode variableNode:
                    return FormatVariable(variableNode);

                case UnaryOperatorNode unaryNode:
                    return FormatUnary(unaryNode);

                case BinaryOperatorNode binaryNode:
                    return FormatBinary(binaryNode);

                case TernaryOperatorNode ternaryNode:
                    return "(" + Format(ternaryNode.Condition) + ") ? ("
                        + Format(ternaryNode.IfTrue) + ") : ("
                        + Format(ternaryNode.IfFalse) + ")";

                case FunctionNode functionNode:
                    return functionNode.Name + "("
                        + string.Join(",", functionNode.Arguments.Select(Format))
                        + ")";

                case PropertyNode propertyNode:
                    return "@" + propertyNode.Name + "[" + propertyNode.PropertyName + "]";
            }

            return node.ToString();
        }

        private string FormatVariable(VariableNode node)
        {
            switch (node.NodeType)
            {
                case NodeTypes.Voltage:
                    return "V(" + node.Name + ")";

                case NodeTypes.Current:
                    return "I(" + node.Name + ")";

                default:
                    return node.Name;
            }
        }

        private string FormatUnary(UnaryOperatorNode node)
        {
            switch (node.NodeType)
            {
                case NodeTypes.Plus:
                    return "+(" + Format(node.Argument) + ")";

                case NodeTypes.Minus:
                    return "-(" + Format(node.Argument) + ")";

                case NodeTypes.Not:
                    return "!(" + Format(node.Argument) + ")";

                default:
                    return node.ToString();
            }
        }

        private string FormatBinary(BinaryOperatorNode node)
        {
            var left = Format(node.Left);
            var right = Format(node.Right);
            switch (node.NodeType)
            {
                case NodeTypes.Add:
                    return "(" + left + ") + (" + right + ")";

                case NodeTypes.Subtract:
                    return "(" + left + ") - (" + right + ")";

                case NodeTypes.Multiply:
                    return "(" + left + ") * (" + right + ")";

                case NodeTypes.Divide:
                    return "(" + left + ") / (" + right + ")";

                case NodeTypes.Modulo:
                    return "(" + left + ") % (" + right + ")";

                case NodeTypes.LessThan:
                    return "(" + left + ") < (" + right + ")";

                case NodeTypes.GreaterThan:
                    return "(" + left + ") > (" + right + ")";

                case NodeTypes.LessThanOrEqual:
                    return "(" + left + ") <= (" + right + ")";

                case NodeTypes.GreaterThanOrEqual:
                    return "(" + left + ") >= (" + right + ")";

                case NodeTypes.Equals:
                    return "(" + left + ") == (" + right + ")";

                case NodeTypes.NotEquals:
                    return "(" + left + ") != (" + right + ")";

                case NodeTypes.And:
                    return "(" + left + ") && (" + right + ")";

                case NodeTypes.Or:
                    return "(" + left + ") || (" + right + ")";

                case NodeTypes.Xor:
                    return "(" + left + ") ^ (" + right + ")";

                case NodeTypes.Pow:
                    return "(" + left + ") ** (" + right + ")";

                default:
                    return node.ToString();
            }
        }
    }
}
