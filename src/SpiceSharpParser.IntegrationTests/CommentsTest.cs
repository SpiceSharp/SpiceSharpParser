using SpiceSharpParser.Model.SpiceObjects;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class CommentsTest : BaseTest
    {
        [Fact]
        public void CommentTest()
        {
            var netlist = ParseNetlistToModel(
                "Comment test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5",
                ".END");

            Assert.Equal("Comment test circuit", netlist.Title);
            Assert.Equal(3, netlist.Statements.Count());
            Assert.True(netlist.Statements.ToArray()[0] is CommentLine);

            Assert.True(netlist.Statements.ToArray()[1] is Component);
            Assert.Equal(" test2", netlist.Statements.ToArray()[1].Comment);

            Assert.True(netlist.Statements.ToArray()[2] is Component);
            Assert.Equal("  test3 ; test4 $ test5", netlist.Statements.ToArray()[2].Comment);
        }
    }
}
