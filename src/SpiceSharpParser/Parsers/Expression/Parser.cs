using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharpBehavioral.Parsers;
using Lexer = SpiceSharpParser.Lexers.Expressions.Lexer;
using TokenType = SpiceSharpParser.Lexers.Expressions.TokenType;

namespace SpiceSharpParser.Parsers.Expression
{
    /// <summary>
    /// Methods for parsing expressions.
    /// </summary>
    /// <remarks>
    /// Code from SpiceSharpBehavioral.
    /// </remarks>
    public static class Parser
    {
        /// <summary>
        /// Parses an expression using the given lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="throw">Throw exception if lexer contains more characters.</param>
        /// <returns>The parsed expression.</returns>
        public static Node Parse(Lexer lexer, bool @throw = false)
        {
            var result = ParseConditional(lexer);
            if (lexer.Type != TokenType.EndOfExpression && @throw)
                throw new ParserException(lexer, "Unrecognized token '{0}'".FormatString(lexer.Content));
            return result;
        }

        private static Node ParseConditional(Lexer lexer)
        {
            var result = ParseConditionalOr(lexer);
            while (lexer.Type == TokenType.Huh)
            {
                lexer.Next();
                var ifTrue = ParseConditional(lexer);
                if (lexer.Type != TokenType.Colon)
                    throw new ParserException(lexer, "Unrecognized token '{0}', expected ':'".FormatString(lexer.Content));
                lexer.Next();
                var ifFalse = ParseConditional(lexer);
                result = Node.Conditional(result, ifTrue, ifFalse);
            }
            return result;
        }

        private static Node ParseConditionalOr(Lexer lexer)
        {
            var result = ParseConditionalAnd(lexer);
            while (lexer.Type == TokenType.Or)
            {
                lexer.Next();
                var right = ParseConditionalAnd(lexer);
                result = Node.Or(result, right);
            }
            return result;
        }

        private static Node ParseConditionalAnd(Lexer lexer)
        {
            var result = ParseEquality(lexer);
            while (lexer.Type == TokenType.And)
            {
                lexer.Next();
                var right = ParseEquality(lexer);
                result = Node.And(result, right);
            }
            return result;
        }

        private static Node ParseEquality(Lexer lexer)
        {
            var result = ParseRelational(lexer);
            while (lexer.Type == TokenType.Equals || lexer.Type == TokenType.NotEquals)
            {
                Node right;
                switch (lexer.Type)
                {
                    case TokenType.Equals:
                        lexer.Next();
                        right = ParseRelational(lexer);
                        result = Node.Equals(result, right);
                        break;
                    case TokenType.NotEquals:
                        lexer.Next();
                        right = ParseRelational(lexer);
                        result = Node.NotEquals(result, right);
                        break;
                }
            }
            return result;
        }

        private static Node ParseRelational(Lexer lexer)
        {
            var result = ParseAdditive(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Type)
                {
                    case TokenType.LessThan:
                        lexer.Next();
                        right = ParseAdditive(lexer);
                        result = Node.LessThan(result, right);
                        break;
                    case TokenType.GreaterThan:
                        lexer.Next();
                        right = ParseAdditive(lexer);
                        result = Node.GreaterThan(result, right);
                        break;
                    case TokenType.LessEqual:
                        lexer.Next();
                        right = ParseAdditive(lexer);
                        result = Node.LessThanOrEqual(result, right);
                        break;
                    case TokenType.GreaterEqual:
                        lexer.Next();
                        right = ParseAdditive(lexer);
                        result = Node.GreaterThanOrEqual(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }

        private static Node ParseAdditive(Lexer lexer)
        {
            var result = ParseMultiplicative(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Type)
                {
                    case TokenType.Plus:
                        lexer.Next();
                        right = ParseMultiplicative(lexer);
                        result = Node.Add(result, right);
                        break;
                    case TokenType.Minus:
                        lexer.Next();
                        right = ParseMultiplicative(lexer);
                        result = Node.Subtract(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }

        private static Node ParseMultiplicative(Lexer lexer)
        {
            var result = ParseUnary(lexer);
            while (true)
            {
                Node right;
                switch (lexer.Type)
                {
                    case TokenType.Times:
                        lexer.Next();
                        right = ParseUnary(lexer);
                        result = Node.Multiply(result, right);
                        break;
                    case TokenType.Divide:
                        lexer.Next();
                        right = ParseUnary(lexer);
                        result = Node.Divide(result, right);
                        break;
                    case TokenType.Mod:
                        lexer.Next();
                        right = ParseUnary(lexer);
                        result = Node.Modulo(result, right);
                        break;
                    default:
                        return result;
                }
            }
        }

        private static Node ParseUnary(Lexer lexer)
        {
            Node argument;
            switch (lexer.Type)
            {
                case TokenType.Plus:
                    lexer.Next();
                    argument = ParseUnary(lexer);
                    return Node.Plus(argument);
                case TokenType.Minus:
                    lexer.Next();
                    argument = ParseUnary(lexer);
                    return Node.Minus(argument);
                default:
                    return ParsePower(lexer);
            }
        }

        private static Node ParsePower(Lexer lexer)
        {
            var result = ParseTerminal(lexer);
            if (lexer.Type == TokenType.Power)
            {
                lexer.Next();
                var right = ParsePower(lexer);
                result = Node.Power(result, right);
            }
            return result;
        }

        private static Node ParseTerminal(Lexer lexer)
        {
            Node result;
            switch (lexer.Type)
            {
                case TokenType.LeftParenthesis:
                    lexer.Next();
                    result = ParseConditional(lexer);
                    if (lexer.Type != TokenType.RightParenthesis)
                        throw new ParserException("Expected closing parenthesis, but found '{0}'".FormatString(lexer.Content));
                    lexer.Next();
                    break;

                case TokenType.Number:
                    result = SpiceHelper.ParseNumber(lexer.Content);
                    lexer.Next();
                    break;

                case TokenType.Identifier:
                    string name = lexer.Content;
                    lexer.Next();
                    if (lexer.Type == TokenType.LeftParenthesis)
                    {
                        // Function call!
                        lexer.Next();

                        string function = null;
                        switch (name.ToLowerInvariant())
                        {
                            case "vr":
                                function = "real";
                                goto case "v";
                            case "vi":
                                function = "imag";
                                goto case "v";
                            case "vm":
                                function = "abs";
                                goto case "v";
                            case "v":
                                // Read the nodes
                                lexer.ContinueWhileNode();
                                result = Node.Voltage(lexer.Content);
                                lexer.Next();
                                if (lexer.Type == TokenType.Comma)
                                {
                                    lexer.Next(); lexer.ContinueWhileNode();
                                    result = Node.Subtract(result, Node.Voltage(lexer.Content));
                                    lexer.Next();
                                }
                                if (lexer.Type != TokenType.RightParenthesis)
                                    throw new ParserException(lexer, "Expected closing parenthesis but found '{0}'".FormatString(lexer.Content));
                                lexer.Next();
                                break;

                            case "ir":
                                function = "real";
                                goto case "i";
                            case "ii":
                                function = "imag";
                                goto case "i";
                            case "im":
                                function = "abs";
                                goto case "i";
                            case "i":
                                // Read the nodes
                                lexer.ContinueWhileNode();
                                result = Node.Current(lexer.Content);
                                lexer.Next();
                                if (lexer.Type != TokenType.RightParenthesis)
                                    throw new ParserException("Expected closing parenthesis but found '{0}'".FormatString(lexer.Content));
                                lexer.Next();
                                break;

                            default:
                                var arguments = new List<Node>(2);
                                while (lexer.Type != TokenType.RightParenthesis)
                                {
                                    arguments.Add(ParseConditional(lexer));

                                    // continue
                                    if (lexer.Type == TokenType.Comma)
                                        lexer.Next();
                                    else if (lexer.Type != TokenType.RightParenthesis)
                                        throw new ParserException("Expected closing parenthesis but found '{0}'".FormatString(lexer.Content));
                                }
                                result = Node.Function(name, arguments);
                                lexer.Next();
                                break;
                        }
                        if (function != null)
                            result = Node.Function(function, new[] { result });
                    }
                    else
                        result = Node.Variable(name);
                    break;

                case TokenType.At:
                    lexer.Next();
                    lexer.ContinueWhileNode();
                    name = lexer.Content;
                    lexer.Next();
                    if (lexer.Type != TokenType.LeftIndex)
                        throw new ParserException("Expected property indexing, but found '{0}'".FormatString(lexer.Content));
                    lexer.Next();
                    lexer.ContinueWhileNode();
                    result = Node.Property(name, lexer.Content);
                    lexer.Next();
                    if (lexer.Type != TokenType.RightIndex)
                        throw new ParserException("Expected closing indexer, but found '{0}'".FormatString(lexer.Content));
                    lexer.Next();
                    break;

                default:
                    throw new ParserException("Invalid value");
            }
            return result;
        }
    }
}
