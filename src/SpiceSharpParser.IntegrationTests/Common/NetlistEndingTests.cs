using SpiceSharpParser.Parsers.Netlist;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class NetlistEndingTests : BaseTests
    {
        [Fact]
        public void When_EndingHasMultipleNewLines_Expect_NoException()
        {
            var netlist = ParseNetlistToModel(
                true,
                true,
                true,
                "Newline test circuit",
                ".END",
                string.Empty,
                string.Empty);
        }

        [Fact]
        public void When_EndingHasMultipleNewLinesNotRequired_Expect_NoException()
        {
            var netlist = ParseNetlistToModel(
                true,
                false,
                true,
                "Newline test circuit",
                ".END",
                string.Empty,
                string.Empty);
        }

        [Fact]
        public void When_EndingHasNewLine_Expect_NoException()
        {
            var netlist = ParseNetlistToModel(
                true,
                true,
                true,
                "Newline test circuit",
                ".END",
                string.Empty);
        }

        [Fact]
        public void When_EndingHasNewLineNotRequired_Expect_NoException()
        {
            var netlist = ParseNetlistToModel(
                true,
                false,
                true,
                "Newline test circuit",
                ".END",
                string.Empty);
        }

        [Fact]
        public void When_EndingHasNoNewLineRequired_Expect_Exception()
        {
            Assert.Throws<ParseException>(() => ParseNetlistToModel(true, true, true, "Newline test circuit", ".END"));
        }

        [Fact]
        public void When_EndingHasNoNewLineRequiredWithoutEnd_Expect_Exception()
        {
            Assert.Throws<ParseException>(() => ParseNetlistToModel(false, true, true, "Newline test circuit"));
        }
    }
}
