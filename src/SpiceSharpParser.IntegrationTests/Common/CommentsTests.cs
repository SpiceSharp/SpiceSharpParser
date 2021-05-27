using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class CommentsTests : BaseTests
    {
        [Fact]
        public void When_CommentsMixed_Expect_Refrence()
        {
            var netlist = ParseNetlist(
                true,
                true,
                "Comment test circuit",
                "* test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5",
                ".END");

            Assert.Equal("Comment test circuit", netlist.Title);
            Assert.Equal(3, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);

            Assert.True(netlist.Statements[1] is Component);
            Assert.True(netlist.Statements[2] is Component);
        }

        [Fact]
        public void When_ThereIsSpaceBeforeStar_Expect_Reference()
        {
            var netlist = ParseNetlist(
                true,
                true,
                "Comment test circuit",
                " * test1",
                "R1 OUT 0 10 ; test2",
                "V1 OUT 0 0 $  test3 ; test4 $ test5",
                ".END");

            Assert.Equal("Comment test circuit", netlist.Title);
            Assert.Equal(3, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);

            Assert.True(netlist.Statements[1] is Component);
            Assert.True(netlist.Statements[2] is Component);
        }

        [Fact]
        public void When_NewLineWithComments_Expect_NoExceptions()
        {
            var netlist = string.Join(
                "\n",
                "Comment test circuit",
                ".SUBCKT A 1 2",
                string.Empty,
                "*******************************************",
                ".ENDS",
                string.Empty,
                "* Copyright (c) 2003-2012",
                string.Empty);

            var exception = Record.Exception(() =>
                ParseNetlist(
                    false,
                    true,
                    netlist));

            Assert.Null(exception);
        }

        [Fact]
        public void When_CommentsSubckt_Expect_NoException()
        {
            var exception = Record.Exception(() =>
               ParseNetlist(
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
                ".end"));

            Assert.Null(exception);
        }

        [Fact]
        public void When_CommentsOnly_Expect_NoException()
        {
            var exception = Record.Exception(() =>
               ParseNetlist(
                false,
                false,
                "**",
                "**",
                ".end"));

            Assert.Null(exception);
        }
    }
}