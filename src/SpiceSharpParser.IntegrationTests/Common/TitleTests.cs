using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class TitleTests : BaseTests
    {
        [Fact]
        public void When_NetlistTitleAndEnd_Expect_Reference()
        {
            var netlist = ParseNetlist(
                true,
                true,
                "Title test circuit",
                ".END");

            Assert.Equal("Title test circuit", netlist.Title);
        }

        [Fact]
        public void When_NetlistEmptyTitleAndEnd_Expect_Reference()
        {
            var netlist = ParseNetlist(
                true,
                true,
                "",
                ".END");

            Assert.Equal("", netlist.Title);
        }

        [Fact]
        public void When_NetlistOnlyTitle_Expect_Reference()
        {
            var netlist = ParseNetlist(
                false,
                true,
                "Title test circuit",
                "");

            Assert.Equal("Title test circuit", netlist.Title);
        }

        [Fact]
        public void When_NetlistOnlyEnd_Expect_Reference()
        {
            var netlist = ParseNetlistToModel(
                true,
                false,
                ".end");

            Assert.Null(netlist.Title);
        }

        [Fact]
        public void When_NetlistWithoutTitle_Expect_Reference()
        {
            var netlist = ParseNetlist(
                true,
                false,
                "v1 1 0 1",
                ".end");

            Assert.Null(netlist.Title);
            Assert.Equal(1, netlist.Statements.Count);
        }

        [Fact]
        public void When_NetlistEmpty_Expect_Reference()
        {
            var netlist = ParseNetlistToModel(
                false,
                false,
                "");

            Assert.Null(netlist.Title);
        }

        [Fact]
        public void When_NetlistEmptyTitleRequired_Expect_Reference()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "");

            Assert.Null(netlist.Title);
        }

        [Fact]
        public void When_NetlistOnlyTitleWithoutNewLine_Expect_Reference()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "1");

            Assert.Equal("1", netlist.Title);
        }
    }
}