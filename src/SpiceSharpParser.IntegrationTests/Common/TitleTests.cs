using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class TitleTests : BaseTests
    {
        [Fact]
        public void TitleAndEnd()
        {
            var netlist = ParseNetlistToModel(
                true,
                true,
                "Title test circuit",
                ".END");

            Assert.Equal("Title test circuit", netlist.Title);
        }

        [Fact]
        public void EmptyTitle()
        {
            var netlist = ParseNetlistToModel(
                true,
                true,
                "",
                ".END");

            Assert.Equal("", netlist.Title);
        }

        [Fact]
        public void OnlyTitle()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "Title test circuit",
                "");

            Assert.Equal("Title test circuit", netlist.Title);
        }

        [Fact]
        public void OnlyEnd()
        {
            var netlist = ParseNetlistToModel(
                true,
                false,
                ".end");

            Assert.Null(netlist.Title);
        }

        [Fact]
        public void WithoutTitle()
        {
            var netlist = ParseNetlistToModel(
                true,
                false,
                "v1 1 0 1",
                ".end");

            Assert.Null(netlist.Title);
            Assert.Equal(1, netlist.Statements.Count);
        }

        [Fact]
        public void Nothing()
        {
            var netlist = ParseNetlistToModel(
                false,
                false,
                "");

            Assert.Null(netlist.Title);
        }

        [Fact]
        public void EmptyWithTitle()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "");

            Assert.Null(netlist.Title);
        }

        [Fact]
        public void OnlyTitleWithoutNewline()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "1");

            Assert.Equal("1", netlist.Title);
        }
    }
}
