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
    }
}