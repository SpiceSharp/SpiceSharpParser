using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Exceptions;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class LineNumbersTests : BaseTests
    {
        [Fact]
        public void When_LineNumbers_Expect_Reference()
        {
            var netlist = ParseNetlistToModel(
                false,
                true,
                "Line numbers test circuit",
                "* test1",
                "* test2",
                "",
                "R1 OUT 0",
                " + 10",
                "* test 3",
                "V1 OUT 0 0 $  test3.3 ; test4 $ test5",
                ".END");

            Assert.Equal("Line numbers test circuit", netlist.Title);
            Assert.Equal(5, netlist.Statements.Count());

            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.True(netlist.Statements[0].LineNumber == 2);

            Assert.True(netlist.Statements[1] is CommentLine);
            Assert.True(netlist.Statements[1].LineNumber == 3);

            Assert.True(netlist.Statements[2] is Component);
            Assert.True(netlist.Statements[2].LineNumber == 5);

            Assert.True(netlist.Statements[3] is CommentLine);
            Assert.True(netlist.Statements[3].LineNumber == 7);

            Assert.True(netlist.Statements[4] is Component);
            Assert.True(netlist.Statements[4].LineNumber == 8);
        }

        [Fact]
        public void When_BugInExpression_Expect_Reference()
        {
            try
            {
                ParseNetlistToModel(
                    false,
                    true,
                    "Line numbers test circuit",
                    "* test1",
                    "* test2",
                    "",
                    "R1 OUT 0",
                    "",
                    " + {1/a}",
                    "* test 3",
                    "V1 OUT 0 0 $  test3.3 ; test4 $ test5",
                    ".END");
            }
            catch (GeneralReaderException ex)
            {
                Assert.Equal(7, ex.LineNumber);
                return;
            }

            Assert.False(true);
        }
    }
}
