using SpiceSharpParser.Connector.Evaluation;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.Connector.Evaluation
{
    public class SpiceExpressionTest
    {
        [Fact]
        public void ParseWithUserFunction()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>(),
                UserFunctions = new Dictionary<string, Func<string[], double>>()
            };

            parser.UserFunctions.Add("v", (args) => {
                if (args.Length == 2)
                {
                    return 4;
                }

                if (args.Length == 1)
                {
                    return 6;
                }
                return -1;
            });

            // act
            var result = parser.Parse("0.5 * (v(out1, ref) + v(out2))");

            // assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void ParseWithUserFunctionNoArgument()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>(),
                UserFunctions = new Dictionary<string, Func<string[], double>>()
            };

            Random rand = new Random(Environment.TickCount);

            double randomVal = 0;

            parser.UserFunctions.Add("random", (args) => {
                randomVal = rand.Next() * 1000;
                return randomVal;
            });

            // act
            var result = parser.Parse("random() + 1");

            // assert
            Assert.Equal(randomVal + 1, result);
        }

        [Fact]
        public void ParseWithUnknownParameter()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>(),
                UserFunctions = new Dictionary<string, Func<string[], double>>()
            };

            // act and assert
            Assert.Throws<Exception>(() => parser.Parse("x + 1"));
        }

        [Fact]
        public void ParseWitKnownParameter()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>() { { "x", 1 } },
                UserFunctions = new Dictionary<string, Func<string[], double>>()
            };

            // act and assert
            Assert.Equal(2, parser.Parse("x + 1"));
        }

        [Fact]
        public void ParseWithBuildinFunction()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>() { { "x", 1 } },
                UserFunctions = new Dictionary<string, Func<string[], double>>()
            };

            // act and assert
            Assert.Equal(1, parser.Parse("sin(0) + 1"));
        }
    }
}
