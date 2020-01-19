using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class EndTests : BaseTests
    {
        [Fact]
        public void EndingNoException()
        {
            var netlist = ParseNetlistToModel(
                true,
                true,
                "End test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5",
                ".END");
        }

        [Fact]
        public void EndingNoExceptionOptional()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "End test circuit",
                ".END");
        }

        [Fact]
        public void NoEndingNoException()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "End test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5\n");
        }

        [Fact]
        public void NoEndingException()
        {
            var text = string.Join(Environment.NewLine, "End test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5\n");
            var parser = new SpiceParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var result = parser.ParseNetlist(text);
            Assert.True(result.ValidationResult.Parsing.HasError);
        }
    }
}