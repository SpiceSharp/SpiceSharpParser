using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Linq;
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
            Assert.True(netlist.Statements[0].LineInfo.LineNumber == 2);

            Assert.True(netlist.Statements[1] is CommentLine);
            Assert.True(netlist.Statements[1].LineInfo.LineNumber == 3);

            Assert.True(netlist.Statements[2] is Component);
            Assert.True(netlist.Statements[2].LineInfo.LineNumber == 5);

            Assert.True(netlist.Statements[3] is CommentLine);
            Assert.True(netlist.Statements[3].LineInfo.LineNumber == 7);

            Assert.True(netlist.Statements[4] is Component);
            Assert.True(netlist.Statements[4].LineInfo.LineNumber == 8);
        }

        [Fact(Skip = "Will be fixed in new release")]
        public void When_BugInExpression_Expect_Reference()
        {
            var result = ParseNetlist(
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

            Assert.False(result.ValidationResult.HasError);
            Assert.Equal(7, result.ValidationResult.First().LineInfo.LineNumber);
        }
    }
}