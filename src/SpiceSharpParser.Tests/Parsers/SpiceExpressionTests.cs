using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Parsers.Expression;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation.Functions;
using Xunit;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.Tests.Parsers
{
    public class SpiceExpressionTests
    {
        [Fact]
        public void When_ExpressionHasCustomFunctionWithArguments_Expect_Reference()
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
                        ObjectArgsLogic = (image, args, evaluator, context) =>
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
            var result = parser.Parse("0.5 * (v(out1, ref) + v(out2))",
                new ExpressionParserContext() { Functions = functions }).Value(new ExpressionEvaluationContext()); 

            // assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void When_ExpressionHasCustomFunctionWithoutArguments_Expect_Reference()
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
                        ObjectArgsLogic = (image, args, evaluator, context) =>
                        {
                            randomVal = rand.Next() * 1000;
                            return randomVal;
                        }
                    }
                }
            };

            // act
            var result = parser.Parse("random() + 1", new ExpressionParserContext() { Functions = functions }).Value(new ExpressionEvaluationContext());

            // assert
            Assert.Equal(randomVal + 1, result);
        }

        [Fact]
        public void When_ExpressionHasUnknownParameter_Expect_Exception()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Throws<UnknownParameterException>(() => parser.Parse("x + 1", new ExpressionParserContext()).Value(new ExpressionEvaluationContext()));
        }

        [Fact]
        public void When_ExpressionHasKnownParameter_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var context = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            context.SetParameter("x", 1);

            // act and assert
            Assert.Equal(2, parser.Parse("x + 1", new ExpressionParserContext() {} ).Value(new ExpressionEvaluationContext() { ExpressionContext = context}));
        }

        [Fact]
        public void When_ExpressionHasSinFunction_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            var context = new ExpressionContext();
            context.SetParameter("x", 1);

            // act and assert
            Assert.Equal(1, parser.Parse("sin(0) + 1", new ExpressionParserContext() { Functions = context.Functions }).Value(new ExpressionEvaluationContext() { ExpressionContext = context }));
        }

        [Fact]
        public void When_ExpressionHasParameters_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            var parseResult = parser.Parse("1 + N + R + s", new ExpressionParserContext());

            Assert.Equal(3, parseResult.FoundParameters.Count);
        }

        [Fact]
        public void When_ExpressionHasSpaces_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(3, parser.Parse(" 2 + 1 ", new ExpressionParserContext()).Value(new ExpressionEvaluationContext()));
        }

        [Fact]
        public void When_ExpressionHasConstants_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            var context = new SpiceExpressionContext(SpiceExpressionMode.HSpice);
            context.SetParameter("PI", Math.PI);
            context.SetParameter("e", Math.E);

            // act and assert
            Assert.Equal((2 * Math.PI) + (2 * Math.E), parser.Parse("PI + e + pi + E", new ExpressionParserContext() { })
                .Value(new ExpressionEvaluationContext() { ExpressionContext = context }));
        }

        [Fact]
        public void When_ExpressionHasMinusValue_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(-1, parser.Parse("-1", new ExpressionParserContext()).Value(new ExpressionEvaluationContext()));
        }

        [Fact]
        public void When_ExpressionHasConditional_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var context = new SpiceExpressionContext(SpiceExpressionMode.HSpice);
            context.SetParameter("TEMP", 26);
            // act and assert
            Assert.Equal(2.52e-9, parser.Parse("TEMP == 26 ? 2.52e-9 : 2.24e-9", new ExpressionParserContext() {  }).Value(new ExpressionEvaluationContext() { ExpressionContext = context }));

            context.SetParameter("TEMP", 27);
            Assert.Equal(2.24e-9, parser.Parse("TEMP == 26 ? 2.52e-9 : 2.24e-9", new ExpressionParserContext() {  }).Value(new ExpressionEvaluationContext() { ExpressionContext = context }));
        }

        [Fact]
        public void When_ExpressionHasUnicodeU_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(12.3 * 1e-6, parser.Parse("12.3μ", new ExpressionParserContext()).Value(new ExpressionEvaluationContext()));
        }

        [Fact]
        public void When_ExpressionHasMonkeyFunction_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var functions = new Dictionary<string, Function>()
            {
                { "@",  new Function()
                {
                    ArgumentsCount = 2,
                    VirtualParameters = true,
                    ObjectArgsLogic = (image, args, evaluator, context) =>
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
            Assert.Equal(8, parser.Parse(" 2 + @obj1[param] + @obj2[param2] + @obj3.subObj[param3]", 
                new ExpressionParserContext() { Functions = functions }).Value(new ExpressionEvaluationContext()));
        }
    }
}
