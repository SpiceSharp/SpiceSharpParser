using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Parser.Netlist;
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
                true,
                true,
                "Comment test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5",
                ".END");

            Assert.Equal("Comment test circuit", netlist.Title);
            Assert.Equal(3, netlist.Statements.Count());
            Assert.True(netlist.Statements.ToArray()[0] is CommentLine);

            Assert.True(netlist.Statements.ToArray()[1] is Component);
            Assert.True(netlist.Statements.ToArray()[2] is Component);
        }


        [Fact]
        public void StrangeNoExceptionTest()
        {
            var netlist = ParseNetlistToModel(
                true,
                true,
                "*",
                "*$",
                ".subckt tddsdsd202 inp inn out vcc vee",
                "*;",
                ".MODEL D_b D",
                "+ RS = 1.0000E-1 ; comment2",
                "+ CJO = 1.0000E-13 $ comment1",
                "+ IS = 100e-15",
                ".ends",
                ".end");
        }

        [Fact]
        public void StrangeNoExceptionTest2()
        {
            try
            {
                var netlist = ParseNetlistToModel(
                    false,
                    true,
                    "",
                    "*$");
            }
            catch (ParseException ex)
            {
                Assert.True(ex.Message.Contains("NEWLINE"));
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public void StrangeNoExceptionTest3()
        {
            var netlist = ParseNetlistToModel(
                false,
                false,
                "**",
                "**",
                ".end");
        }
    }
}
