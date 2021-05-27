using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class EndTests : BaseTests
    {
        [Fact]
        public void EndingNoException()
        {
            var exception = Record.Exception(() => ParseNetlist(
                true,
                true,
                "End test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5",
                ".END"));

            Assert.Null(exception);
        }

        [Fact]
        public void EndingNoExceptionOptional()
        {
            var exception = Record.Exception(() => ParseNetlist(
               false,
                true,
                "End test circuit",
                ".END"));

            Assert.Null(exception);
        }

        [Fact]
        public void NoEndingNoException()
        {
            var exception = Record.Exception(() => ParseNetlist(
                false,
                true,
                "End test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5\n"));

            Assert.Null(exception);
        }

        [Fact]
        public void NoEndingException()
        {
            var text = string.Join(Environment.NewLine, "End test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5\n");

            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var parseResult = parser.ParseNetlist(text);
            Assert.True(parseResult.ValidationResult.HasError);
        }
    }
}