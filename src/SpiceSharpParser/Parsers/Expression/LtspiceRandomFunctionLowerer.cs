using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.Expression
{
    internal static class LtspiceRandomFunctionLowerer
    {
        private const double HashFrequency = 12.9898;
        private const double HashScale = 43758.5453;
        private const double HashOffset = 0.5;
        private const double RandomTransitionStart = 0.125;
        private const double RandomTransitionWidth = 0.75;

        public static bool TryLower(string name, IReadOnlyList<Node> arguments, out Node result)
        {
            result = null;

            if (string.Equals(name, "rand", StringComparison.OrdinalIgnoreCase))
            {
                EnsureExactlyOneArgument(name, arguments);
                result = CreateRand(arguments[0]);
                return true;
            }

            if (string.Equals(name, "random", StringComparison.OrdinalIgnoreCase))
            {
                // Keep the parser's existing zero-argument random() extension.
                if (arguments.Count == 0)
                {
                    return false;
                }

                EnsureExactlyOneArgument(name, arguments);
                result = CreateRandom(arguments[0]);
                return true;
            }

            if (string.Equals(name, "white", StringComparison.OrdinalIgnoreCase))
            {
                EnsureExactlyOneArgument(name, arguments);
                result = CreateWhite(arguments[0]);
                return true;
            }

            return false;
        }

        private static Node CreateRand(Node argument)
        {
            return CreateHash(Floor(argument));
        }

        private static Node CreateRandom(Node argument)
        {
            var integer = Floor(argument);
            var fraction = Node.Subtract(argument, integer);
            Node transition = Node.Divide(
                Node.Subtract(fraction, Node.Constant(RandomTransitionStart)),
                Node.Constant(RandomTransitionWidth));
            transition = Node.Function(
                "limit",
                new Node[] { transition, Node.Constant(0.0), Node.Constant(1.0) });

            return InterpolateHashes(integer, SmoothStep(transition), offset: 0.0);
        }

        private static Node CreateWhite(Node argument)
        {
            var integer = Floor(argument);
            var fraction = Node.Subtract(argument, integer);
            return InterpolateHashes(integer, SmoothStep(fraction), offset: -0.5);
        }

        private static Node InterpolateHashes(Node integer, Node fraction, double offset)
        {
            var current = CreateHash(integer);
            var next = CreateHash(Node.Add(integer, Node.Constant(1.0)));
            var interpolated = Node.Add(
                current,
                Node.Multiply(Node.Subtract(next, current), fraction));

            return offset == 0.0
                ? interpolated
                : Node.Add(interpolated, Node.Constant(offset));
        }

        private static Node SmoothStep(Node value)
        {
            return Node.Multiply(
                Node.Multiply(value, value),
                Node.Subtract(Node.Constant(3.0), Node.Multiply(Node.Constant(2.0), value)));
        }

        private static Node CreateHash(Node integer)
        {
            var sine = Node.Function(
                "sin",
                new[] { Node.Multiply(integer, Node.Constant(HashFrequency)) });
            var value = Node.Add(
                Node.Multiply(sine, Node.Constant(HashScale)),
                Node.Constant(HashOffset));

            return Node.Subtract(value, Floor(value));
        }

        private static Node Floor(Node value)
        {
            return Node.Function("floor", new[] { value });
        }

        private static void EnsureExactlyOneArgument(string name, IReadOnlyList<Node> arguments)
        {
            if (arguments.Count != 1)
            {
                throw new ParserException($"LTspice {name}() expects exactly one argument");
            }
        }
    }
}
