using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Spice.Evaluation.CustomFunctions;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.ModelReader.Spice.Evaluation
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
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            parser.CustomFunctions.Add("v", new SpiceFunction()
            {
                Logic = (args, context) =>
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
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            Random rand = new Random(Environment.TickCount);

            double randomVal = 0;

            parser.CustomFunctions.Add("random", new SpiceFunction
            {
                Logic = (args, context) =>
                {
                    randomVal = rand.Next() * 1000;
                    return randomVal;
                }
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
                CustomFunctions = new Dictionary<string, SpiceFunction>()
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
                CustomFunctions = new Dictionary<string, SpiceFunction>()
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
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            // act and assert
            Assert.Equal(1, parser.Parse("sin(0) + 1"));
        }

        [Fact]
        public void ParseWithSpace()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>() { },
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            // act and assert
            Assert.Equal(3, parser.Parse(" 2 + 1 "));
        }

        [Fact]
        public void ParseWithComma()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>() { },
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            // act and assert
            Assert.Equal(2.1, parser.Parse("2,1"));
        }

        [Fact]
        public void ParseWithConstants()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>() { },
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            // act and assert
            Assert.Equal((2 * Math.PI) + (2 * Math.E), parser.Parse("PI + e + pi + E"));
        }

        [Fact]
        public void ParseWithReference()
        {
            // arrange
            var parser = new SpiceExpression
            {
                Parameters = new Dictionary<string, double>() { },
                CustomFunctions = new Dictionary<string, SpiceFunction>()
            };

            parser.CustomFunctions.Add("@", new SpiceFunction()
            {
                Logic = (args, context) =>
                {
                    if (args[1].ToString() == "obj1" && args[0].ToString() == "param")
                    {
                        return 1;
                    }
                    if (args[1].ToString() == "obj2" && args[0].ToString() == "param2")
                    {
                        return 2;
                    }

                    if (args[1].ToString() == "obj3.subObj" && args[0].ToString() == "param3")
                    {
                        return 3;
                    }

                    return 0;
                }
            });

            // act and assert
            Assert.Equal(8, parser.Parse(" 2 + @obj1[param] + @obj2[param2] + @obj3.subObj[param3]"));
        }
    }
}
