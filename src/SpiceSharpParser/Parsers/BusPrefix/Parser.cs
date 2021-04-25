using SpiceSharpParser.Lexers.BusPrefix;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Parsers.BusPrefix
{
    public class Parser
    {
        public Node Parse(Lexer lexer)
        {
            lexer.ReadToken();
            return ParseBusPrefix(lexer);
        }

        private Node ParseBusPrefix(Lexer lexer)
        {
            if (lexer.Token == TokenType.LessThan)
            {
                lexer.ReadToken();
                if (lexer.Token == TokenType.Times)
                {
                    lexer.ReadToken();
                    var number = ParseNumber(lexer);
                    if (lexer.Token == TokenType.GreaterThan)
                    {
                        lexer.ReadToken();

                        return new Prefix { Value = number, Nodes = ParseTemplateContent(lexer) };
                    }
                    else
                    {
                        throw new Exception("> expected");
                    }
                }
                else
                {
                    throw new Exception("Expected *");
                }
            }
            else if (lexer.Token == TokenType.Letter || lexer.Token == TokenType.Digit)
            {
                var result = new PrefixNodeName() { Name = ParseName(lexer) };
                return result;
            }
            else
            {
                throw new Exception("Wrong token for node");
            }
        }

        private List<Node> ParseTemplateContent(Lexer lexer)
        {
            var result = new List<Node>();
            if (lexer.Token == TokenType.LeftParenthesis)
            {
                lexer.ReadToken();

                if (lexer.Token == TokenType.Letter || lexer.Token == TokenType.Digit)
                {
                    var identifier = ParseName(lexer);
                    result.Add(new PrefixNodeName() { Name = identifier });

                    while (lexer.Token == TokenType.Comma || lexer.Token == TokenType.Space)
                    {
                        lexer.ReadToken();
                        var node = ParseBusPrefix(lexer);
                        result.Add(node);
                    }

                    if (lexer.Token != TokenType.RightParenthesis)
                    {
                        throw new Exception("Missing right parenthesis");
                    }

                    lexer.ReadToken(false);
                }
            }
            else
            {
                if (lexer.Token == TokenType.Letter || lexer.Token == TokenType.Digit)
                {
                    result.Add(new PrefixNodeName() { Name = ParseName(lexer) });
                }
                else
                {
                    if (lexer.Token != TokenType.EndOfExpression)
                    {
                        throw new Exception("Wrong prefix");
                    }
                }
            }

            return result;
        }

        private int ParseNumber(Lexer lexer)
        {
            var numberString = ReadToken(lexer, TokenType.Digit);
            return int.Parse(numberString);
        }

        private string ReadToken(Lexer lexer, TokenType type)
        {
            string result = string.Empty;
            while (lexer.Token == type)
            {
                result += lexer.Current;
                lexer.ReadToken(false);
            }

            return result;
        }

        private string ParseName(Lexer lexer)
        {
            string result = string.Empty;
            while (lexer.Token == TokenType.Letter || lexer.Token == TokenType.Digit)
            {
                result += lexer.Current;
                lexer.ReadToken(false);
            }

            return result;
        }
    }
}
