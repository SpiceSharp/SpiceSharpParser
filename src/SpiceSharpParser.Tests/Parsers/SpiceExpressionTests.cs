using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.Parsers.Expression;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.Parsers
{
    public class SpiceExpressionTests
    {
        public class CustomFunction : IFunction<string, double>
        {
            public CustomFunction()
            {
                ArgumentsCount = 2;
            }

            public double Logic(string image, string[] args, IEvaluator evaluator, ExpressionContext context)
            {
                return 0;
            }

            public int ArgumentsCount { get; set; } 
            public bool Infix { get; set; }
            public string Name { get; set; }
            public Type ArgumentType => typeof(string);
            public Type OutputType { get; }
        }

        [Fact]
        public void When_ExpressionHasUnknownParameter_Expect_Exception()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Throws<UnknownParameterException>(() => parser.Parse("x + 1", new ExpressionParserContext()).Value(new ExpressionEvaluationContext() { ExpressionContext = new ExpressionContext() }));
        }

        [Fact]
        public void When_ExpressionHasStringFunctionWithParameters_Expect_Exception()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            var context = new ExpressionEvaluationContext() {ExpressionContext = new ExpressionContext()};
            context.ExpressionContext.AddFunction("v", new CustomFunction());
            var parserContext = new ExpressionParserContext();
            parserContext.Functions.Add("v", new List<IFunction>() { new CustomFunction()});

            // act and assert
            parser.Parse("1 + v(2,0) + 3", parserContext).Value(context);
        }

        [Fact]
        public void When_ExpressionHasKnownParameter_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            var context = new SpiceExpressionContext(SpiceExpressionMode.Spice3f5);
            context.SetParameter("x", 1);

            // act and assert
            Assert.Equal(2, parser.Parse("x + 1", new ExpressionParserContext()).Value(
                new ExpressionEvaluationContext() { ExpressionContext = context, Evaluator = new SpiceEvaluator() }));
        }

        [Fact]
        public void When_ExpressionHasSinFunction_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            var context = new ExpressionContext();
            context.CreateCommonFunctions();
            context.SetParameter("x", 1);

            // act and assert
            Assert.Equal(1, parser.Parse("sin(0) + 1", new ExpressionParserContext(context.Functions)).Value(new ExpressionEvaluationContext() { ExpressionContext = context }));
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
            Assert.Equal((2 * Math.PI) + (2 * Math.E), parser.Parse("PI + e + pi + E", new ExpressionParserContext() {  })
                .Value(new ExpressionEvaluationContext() { ExpressionContext = context, Evaluator = new SpiceEvaluator() }));
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
            Assert.Equal(2.52e-9, parser.Parse("TEMP == 26 ? 2.52e-9 : 2.24e-9", new ExpressionParserContext()).Value(new ExpressionEvaluationContext() { Evaluator = new SpiceEvaluator(), ExpressionContext = context }));

            context.SetParameter("TEMP", 27);
            Assert.Equal(2.24e-9, parser.Parse("TEMP == 26 ? 2.52e-9 : 2.24e-9", new ExpressionParserContext()).Value(new ExpressionEvaluationContext() { Evaluator = new SpiceEvaluator(), ExpressionContext = context }));
        }

        [Fact]
        public void When_ExpressionHasUnicodeU_Expect_Reference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(12.3 * 1e-6, parser.Parse("12.3µ", new ExpressionParserContext()).Value(new ExpressionEvaluationContext()));
        }
    }
}
