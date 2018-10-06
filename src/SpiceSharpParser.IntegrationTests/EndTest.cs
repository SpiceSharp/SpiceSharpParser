using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class EndTest : BaseTest
    {
        [Fact]
        public void EndingNoExceptionTest()
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
        public void EndingNoExceptionOptionalTest()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "End test circuit",
                ".END");
        }

        [Fact]
        public void NoEndingNoExceptionTest()
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
        public void NoEndingExceptionTest()
        {
            Assert.Throws<Exception>(() =>
                ParseNetlistToModel(
                    true,
                    true,
                    "End test circuit",
                    "* test1",
                    "R1 OUT 0 10 ; test2",
                    "V1 OUT 0 0 $  test3 ; test4 $ test5\n")
               );
        }
    }
}
