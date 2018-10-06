using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Parsers.Expression;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SpiceSharpParser.Common.Evaluation.Functions;
using Xunit;

namespace SpiceSharpParser.Tests.Parsers
{
    public class SpiceExpressionTest
    {
        [Fact]
        public void ParseWithUserFunction()
        {
            // arrange
            var parser = new SpiceExpressionParser(false);

            var functions = new Dictionary<string, Function>()
            {
                {
                    "v",
                    new Function()
                    {
                        ArgumentsCount = -1,
                        VirtualParameters = true,
                        Logic = (image, args, evaluator) =>
                        {
                            if (args.Length == 2)
                            {
                                return 4;
                            }

                            if (args.Length == 1)
                            {
                                return 6;
                            }

                            return -1;
                        }
                    }
                }
            };

            // act
            var result = parser.Parse("0.5 * (v(out1, ref) + v(out2))", new ExpressionParserContext() { Functions = functions }).Value();

            // assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void ParseWithUserFunctionNoArgument()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            Random rand = new Random(Environment.TickCount);

            double randomVal = 0;
            var functions = new Dictionary<string, Function>()
            {
                {
                    "random",
                    new Function
                    {
                        Logic = (image, args, evaluator) =>
                        {
                            randomVal = rand.Next() * 1000;
                            return randomVal;
                        }
                    }
                }
            };

            // act
            var result = parser.Parse("random() + 1", new ExpressionParserContext() { Functions = functions }).Value();

            // assert
            Assert.Equal(randomVal + 1, result);
        }

        [Fact]
        public void ParseWithUnknownParameter()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Throws<UnknownParameterException>(() => parser.Parse("x + 1", new ExpressionParserContext()));
        }

        [Fact]
        public void ParseWitKnownParameter()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var parameters = new Dictionary<string, Expression>();
            parameters["x"] = new ConstantExpression(1);

            // act and assert
            Assert.Equal(2, parser.Parse("x + 1", new ExpressionParserContext() { Parameters = parameters } ).Value());
        }

        [Fact]
        public void ParseWithBuildinFunction()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var parameters = new Dictionary<string, Expression>();
            parameters["x"] = new ConstantExpression(1);

            var functions = new Dictionary<string, Function>();
            functions["sin"] = MathFunctions.CreateSin();

            // act and assert
            Assert.Equal(1, parser.Parse("sin(0) + 1", new ExpressionParserContext() { Parameters = parameters, Functions = functions}).Value());
        }

        [Fact]
        public void ParseWithParameters()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            var parseResult = parser.Parse("1 + N + R + s", new ExpressionParserContext(), false);

            Assert.Equal(3, parseResult.FoundParameters.Count);
        }

        [Fact]
        public void ParseWithSpace()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(3, parser.Parse(" 2 + 1 ", new ExpressionParserContext()).Value());
        }

        [Fact]
        public void ParseWithComma()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(2.1, parser.Parse("2,1", new ExpressionParserContext()).Value());
        }

        [Fact]
        public void ParseWithConstants()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var parameters = new Dictionary<string, Expression>(StringComparerFactory.Create(false));
            parameters["PI"] = new ConstantExpression(Math.PI);
            parameters["e"] = new ConstantExpression(Math.E);

            // act and assert
            Assert.Equal((2 * Math.PI) + (2 * Math.E), parser.Parse("PI + e + pi + E", new ExpressionParserContext() { Parameters = parameters }).Value());
        }

        [Fact]
        public void ParseMinus()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(-1, parser.Parse("-1", new ExpressionParserContext()).Value());
        }

        [Fact]
        public void ParseConditional()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var parameters = new Dictionary<string, Expression>(StringComparerFactory.Create(false));
            parameters["TEMP"] = new ConstantExpression(26);
            // act and assert
            Assert.Equal(2.52e-9, parser.Parse("TEMP == 26 ? 2.52e-9 : 2.24e-9", new ExpressionParserContext() { Parameters = parameters }).Value());

            parameters["TEMP"] = new ConstantExpression(27);
            Assert.Equal(2.24e-9, parser.Parse("TEMP == 26 ? 2.52e-9 : 2.24e-9", new ExpressionParserContext() { Parameters = parameters }).Value());
        }

        [Fact]
        public void UnicodeU()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(12.3 * 1e-6, parser.Parse("12.3μ", new ExpressionParserContext()).Value());
        }

        [Fact]
        public void ParseWithReference()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var functions = new Dictionary<string, Function>()
            {
                { "@",  new Function()
                {
                    ArgumentsCount = 2,
                    VirtualParameters = true,
                    Logic = (image, args, evaluator) =>
                    {
                        if (args[0].ToString() == "obj1" && args[1].ToString() == "param")
                        {
                            return 1;
                        }
                        if (args[0].ToString() == "obj2" && args[1].ToString() == "param2")
                        {
                            return 2;
                        }

                        if (args[0].ToString() == "obj3.subObj" && args[1].ToString() == "param3")
                        {
                            return 3;
                        }

                        return 0;
                    }
                }}
            };

            // act and assert
            Assert.Equal(8, parser.Parse(" 2 + @obj1[param] + @obj2[param2] + @obj3.subObj[param3]", new ExpressionParserContext() { Functions = functions }).Value());
        }
    }
}
