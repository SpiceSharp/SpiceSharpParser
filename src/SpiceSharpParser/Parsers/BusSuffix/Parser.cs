using SpiceSharpParser.Lexers.BusSuffix;
using System;

namespace SpiceSharpParser.Parsers.BusSuffix
{
    public class Parser
    {
        public Suffix Parse(Lexer lexer)
        {
            lexer.ReadToken();
            return ParseBusSuffix(lexer);
        }

        private Suffix ParseBusSuffix(Lexer lexer)
        {
            var suffix = new Suffix();
            suffix.Name = ParseName(lexer);

            while (lexer.Token == TokenType.LessThan)
            {
                var dimension = new SuffixDimension();

                do
                {
                    lexer.ReadToken();
                    var node = ParseSuffixNode(lexer);

                    if (node != null)
                    {
                        dimension.Nodes.Add(node);
                    }
                }
                while (lexer.Token == TokenType.Comma || lexer.Token == TokenType.Space);

                if (lexer.Token != TokenType.GreaterThan)
                {
                    throw new Exception("Wrong suffix");
                }

                lexer.ReadToken();

                suffix.Dimensions.Add(dimension);
            }

            return suffix;
        }

        private Node ParseSuffixNode(Lexer lexer)
        {
            if (lexer.Token == TokenType.LeftParenthesis)
            {
                // A<(
                lexer.ReadToken();
                var start = ParseNumber(lexer);

                if (lexer.Token != TokenType.Colon)
                {
                    throw new Exception("Wrong suffix");
                }

                lexer.ReadToken();
                var stop = ParseNumber(lexer);

                // A<(1:2
                int? step = null;
                if (lexer.Token == TokenType.Colon)
                {
                    // A<(1:2:
                    lexer.ReadToken();

                    step = ParseNumber(lexer);

                    // A<(1:2:3)
                    if (lexer.Token != TokenType.RightParenthesis)
                    {
                        throw new Exception("Wrong suffix");
                    }
                }

                lexer.ReadToken();

                // A<(1:2:3)*2
                if (lexer.Token != TokenType.Times)
                {
                    throw new Exception("Wrong suffix");
                }

                lexer.ReadToken();
                int times = ParseNumber(lexer);

                return new RangeNode() { Start = start, Stop = stop, Step = step, Multiply = times };
            }
            else
            {
                var start = ParseNumber(lexer);

                if (lexer.Token == TokenType.Colon)
                {
                    // start:stop
                    lexer.ReadToken();
                    var stop = ParseNumber(lexer);

                    if (lexer.Token == TokenType.Colon)
                    {
                        lexer.ReadToken();

                        // start:stop:step
                        var step = ParseNumber(lexer);
                        return new RangeNode() { Start = start, Stop = stop, Step = step };
                    }
                    else
                    {
                        if (lexer.Token == TokenType.Times)
                        {
                            // start:stop*times
                            lexer.ReadToken();
                            var multiply = ParseNumber(lexer);
                            return new RangeNode() { Start = start, Stop = stop, Multiply = multiply };
                        }

                        return new RangeNode() { Start = start, Stop = stop };
                    }
                }
                else
                {
                    if (lexer.Token == TokenType.Times)
                    {
                        return new RangeNode() { Start = start, Stop = start };
                    }
                    else
                    {
                        return new NumberNode() { Node = start };
                    }
                }
            }
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
