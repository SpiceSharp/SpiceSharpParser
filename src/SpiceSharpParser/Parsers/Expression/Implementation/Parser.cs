using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Lexers.Expressions;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.Expression.Implementation
{
    /// <summary>
    /// A parser that parses Spice expressions.
    /// </summary>
    /// <remarks>
    /// This is a recursive descent parser.
    /// </remarks>
    public class Parser
    {
        /// <summary>
        /// Parses the expression using a lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The value of the lexed expression.</returns>
        /// <exception cref="Exception">Invalid expression.</exception>
        public Node Parse(Lexer lexer)
        {
            lexer.ReadToken();
            var result = ParseConditional(lexer);
            return result;
        }

        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The value of the lexed expression.</returns>
        /// <exception cref="Exception">Invalid expression.</exception>
        public Node Parse(string text)
        {
            return Parse(new Lexer(text));
        }

        private Node ParseConditional(Lexer lexer)
        {
            var result = ParseConditionalOr(lexer);
            while (lexer.Token == TokenType.Huh)
            {
                lexer.ReadToken();
                var ifTrue = ParseConditional(lexer);
                if (lexer.Token != TokenType.Colon)
                {
                    throw new Exception("Invalid conditional");
                }

                lexer.ReadToken();
                var ifFalse = ParseConditional(lexer);
                result = Node.Conditional(result, ifTrue, ifFalse);
            }

            return result;
        }

        private Node ParseConditionalOr(Lexer lexer)
        {
            var result = ParseConditionalAnd(lexer);
            while (lexer.Token == TokenType.Or)
            {
                lexer.ReadToken();
                var right = ParseConditionalAnd(lexer);
                result = Node.Or(result, right);
            }

            return result;
        }

        private Node ParseConditionalAnd(Lexer lexer)
        {
            var result = ParseEquality(lexer);
            while (lexer.Token == TokenType.And)
            {
                lexer.ReadToken();
                var right = ParseEquality(lexer);
                result = Node.And(result, right);
            }

            return result;
        }

        private Node ParseEquality(Lexer lexer)
        {
            var result = ParseRelational(lexer);
            while (lexer.Token == TokenType.Equals || lexer.Token == TokenType.NotEquals)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.Equals:
                        lexer.ReadToken();
                        right = ParseRelational(lexer);
                        result = Node.Equals(result, right);
                        break;
                    case TokenType.NotEquals:
                        lexer.ReadToken();
                        right = ParseRelational(lexer);
                        result = Node.NotEquals(result, right);
                        break;
                }
            }

            return result;
        }

        private Node ParseRelational(Lexer lexer)
        {
            var result = ParseAdditive(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.LessThan:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.LessThan(result, right);
                        break;
                    case TokenType.GreaterThan:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.GreaterThan(result, right);
                        break;
                    case TokenType.LessEqual:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.LessThanOrEqual(result, right);
                        break;
                    case TokenType.GreaterEqual:
                        lexer.ReadToken();
                        right = ParseAdditive(lexer);
                        result = Node.GreaterThanOrEqual(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }

        private Node ParseAdditive(Lexer lexer)
        {
            var result = ParseMultiplicative(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.Plus:
                        lexer.ReadToken();
                        right = ParseMultiplicative(lexer);
                        result = Node.Add(result, right);
                        break;
                    case TokenType.Minus:
                        lexer.ReadToken();
                        right = ParseMultiplicative(lexer);
                        result = Node.Subtract(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }

        private Node ParseMultiplicative(Lexer lexer)
        {
            var result = ParseUnary(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Token)
                {
                    case TokenType.Times:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Node.Multiply(result, right);
                        break;
                    case TokenType.Divide:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Node.Divide(result, right);
                        break;
                    case TokenType.Mod:
                        lexer.ReadToken();
                        right = ParseUnary(lexer);
                        result = Node.Modulo(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }

        private Node ParseUnary(Lexer lexer)
        {
            Node argument;
            switch (lexer.Token)
            {
                case TokenType.Plus:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Node.Plus(argument);

                case TokenType.Minus:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Node.Minus(argument);

                case TokenType.Bang:
                    lexer.ReadToken();
                    argument = ParseUnary(lexer);
                    return Node.Not(argument);

                default:
                    return ParsePower(lexer);
            }
        }

        private Node ParsePower(Lexer lexer)
        {
            var result = ParseTerminal(lexer);
            if (lexer.Token == TokenType.Power)
            {
                lexer.ReadToken();
                var right = ParsePower(lexer);
                result = Node.Power(result, right);
            }

            return result;
        }

        private Node ParseTerminal(Lexer lexer)
        {
            Node result;
            switch (lexer.Token)
            {
                // Nested
                case TokenType.LeftParenthesis:
                    lexer.ReadToken();
                    result = ParseConditional(lexer);
                    if (lexer.Token != TokenType.RightParenthesis)
                    {
                        throw new Exception("Unclosed parenthesis");
                    }

                    lexer.ReadToken();
                    break;

                // A number
                case TokenType.Number:
                    result = Node.Constant(SpiceSharpBehavioral.Parsers.SpiceHelper.ParseNumber(lexer.Content));
                    lexer.ReadToken();
                    break;

                // Can be a variable or a function call
                case TokenType.Identifier:
                    string name = lexer.Content;
                    lexer.ReadToken();
                    if (lexer.Token == TokenType.LeftParenthesis)
                    {
                        // Function call!
                        string function = null;
                        switch (name.ToLowerInvariant())
                        {
                            case "vr":
                                function = "real"; goto case "v";
                            case "vi":
                                function = "imag"; goto case "v";
                            case "vm":
                                function = "abs"; goto case "v";
                            case "vdb":
                                function = "db"; goto case "v";
                            case "vph":
                                function = "phase"; goto case "v";
                            case "v":
                                // Read the nodes
                                lexer.ReadNode();
                                result = Node.Voltage(lexer.Content);
                                lexer.ReadToken();
                                if (lexer.Token == TokenType.Comma)
                                {
                                    lexer.ReadNode();
                                    result = Node.Subtract(result, Node.Voltage(lexer.Content));
                                    lexer.ReadToken();
                                }

                                if (lexer.Token != TokenType.RightParenthesis)
                                {
                                    throw new Exception("Invalid voltage specifier");
                                }

                                lexer.ReadToken();
                                break;
                            case "ir":
                                function = "real"; goto case "i";
                            case "ii":
                                function = "imag"; goto case "i";
                            case "im":
                                function = "abs"; goto case "i";
                            case "idb":
                                function = "db"; goto case "i";
                            case "iph":
                                function = "phase"; goto case "i";
                            case "i":
                                // Read the name
                                lexer.ReadNode();
                                result = Node.Current(lexer.Content);
                                lexer.ReadToken();
                                if (lexer.Token != TokenType.RightParenthesis)
                                {
                                    throw new Exception("Invalid current specifier");
                                }

                                lexer.ReadToken();
                                break;

                            default:
                                var arguments = new List<Node>(2);

                                // Let's go to the first token after the function call
                                lexer.ReadToken();
                                while (lexer.Token != TokenType.RightParenthesis)
                                {
                                    // Read the argument
                                    arguments.Add(ParseConditional(lexer));

                                    // Continue with another argument
                                    if (lexer.Token == TokenType.Comma)
                                    {
                                        lexer.ReadToken();
                                    }
                                    else if (lexer.Token != TokenType.RightParenthesis)
                                    {
                                        throw new Exception("Invalid function call");
                                    }
                                }

                                result = Node.Function(name, arguments);
                                lexer.ReadToken();
                                break;
                        }

                        if (function != null)
                        {
                            result = Node.Function(function, new[] { result });
                        }
                    }
                    else
                    {
                        result = Node.Variable(name);
                    }

                    break;

                case TokenType.At:
                    lexer.ReadNode();
                    name = lexer.Content;
                    lexer.ReadToken();
                    if (lexer.Token != TokenType.LeftIndex)
                    {
                        throw new Exception("Invalid property identifier");
                    }

                    lexer.ReadNode();
                    result = Node.Property(name, lexer.Content);
                    lexer.ReadToken();
                    if (lexer.Token != TokenType.RightIndex)
                    {
                        throw new Exception("Invalid property identifier");
                    }

                    lexer.ReadToken();
                    break;

                // There is no level below this, so let's throw an exception
                default:
                    throw new Exception("Invalid value");
            }

            return result;
        }
    }
}
