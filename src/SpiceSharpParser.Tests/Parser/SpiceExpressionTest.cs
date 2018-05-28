using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.Parser.Expressions;
using System;
using System.Collections.Generic;
using Xunit;

namespace SpiceSharpParser.Tests.Parser
{
    public class SpiceExpressionTest
    {
        [Fact]
        public void ParseWithUserFunction()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            parser.CustomFunctions.Add("v", new CustomFunction()
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
            var parser = new SpiceExpressionParser();

            Random rand = new Random(Environment.TickCount);

            double randomVal = 0;

            parser.CustomFunctions.Add("random", new CustomFunction
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
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Throws<Exception>(() => parser.Parse("x + 1"));
        }

        [Fact]
        public void ParseWitKnownParameter()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            parser.Parameters["x"] = 1;

            // act and assert
            Assert.Equal(2, parser.Parse("x + 1"));
        }

        [Fact]
        public void ParseWithBuildinFunction()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            parser.Parameters["x"] = 1;

            // act and assert
            Assert.Equal(1, parser.Parse("sin(0) + 1"));
        }

        [Fact]
        public void ParseWithSpace()
        {
            // arrange
            var parser = new SpiceExpressionParser();
            

            // act and assert
            Assert.Equal(3, parser.Parse(" 2 + 1 "));
        }

        [Fact]
        public void ParseWithComma()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(2.1, parser.Parse("2,1"));
        }

        [Fact]
        public void ParseWithConstants()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal((2 * Math.PI) + (2 * Math.E), parser.Parse("PI + e + pi + E"));
        }

        [Fact]
        public void Power()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(8, parser.Parse( "2**3"));
        }

        [Fact]
        public void UnicodeU()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            // act and assert
            Assert.Equal(12.3 * 1e-6, parser.Parse("12.3μ"));
        }

        [Fact]
        public void ParseWithReference()
        {
            // arrange
            var parser = new SpiceExpressionParser();

            parser.CustomFunctions.Add("@", new CustomFunction()
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
