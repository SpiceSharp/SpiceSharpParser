using SpiceSharpParser.Model.Spice.Objects;
using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class IfTest : BaseTest
    {
        [Fact]
        public void MissingEndIf()
        {
            try
            {
                var netlist = ParseNetlistToPostProcessedModel(
                    false,
                    true,
                   "Missing endif",
                   ".IF (a == 0)",
                   "* Comment 1",
                   ".ELSE",
                   "* Comment 2",
                   ".PARAM a = 1",
                   ".END");

                Assert.False(true);
            }
            catch(Exception ex)
            {
            }
        }

        [Fact]
        public void IfElseTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false,
                true,
               "Simplest netlist with if",
               ".IF (a == 0)",
               "* Comment 1",
               ".ELSE",
               "* Comment 2",
               ".ENDIF",
               ".PARAM a = 1",
               ".END");

            Assert.Equal(2, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.Equal("* Comment 2", ((CommentLine)netlist.Statements[0]).Text);

            Assert.True(netlist.Statements[1] is Control);
        }

        [Fact]
        public void IfElseIfElseTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false,
                true,
               "Simplest netlist with if",
               ".IF (a == 0)",
               "* Comment 0",
               ".ELSEIF (a == 1)",
               "* Comment 1",
               ".ELSE",
               "* Comment 2",
               ".ENDIF",
               ".PARAM a = 1",
               ".END");

            Assert.Equal(2, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.Equal("* Comment 1", ((CommentLine)netlist.Statements[0]).Text);

            Assert.True(netlist.Statements[1] is Control);
        }
        [Fact]
        public void ManyIfElseIfElseTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false,
                true,
               "Simplest netlist with if",
               ".IF (a == 0)",
               "* Comment 0",
               ".ELSEIF (a == 1)",
               "* Comment 1",
               ".ELSEIF (a == 2)",
               "* Comment 2",
               ".ELSEIF (a == 3)",
               "* Comment 3",
               ".ELSEIF (a == 4)",
               "* Comment 4",
               ".ELSE",
               "* Comment 5",
               ".ENDIF",
               ".PARAM a = 4",
               ".END");

            Assert.Equal(2, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.Equal("* Comment 4", ((CommentLine)netlist.Statements[0]).Text);

            Assert.True(netlist.Statements[1] is Control);
        }

        [Fact]
        public void NestedIfElseIfElseTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false,
                true,
               "Simplest netlist with if",
               ".IF (a == 0)",
               "* Comment 0",
               ".ELSEIF (a == 1)",
               "* Comment 1",
               ".ELSEIF (a == 2)",
               "* Comment 2",
               ".ELSEIF (a == 3)",
               ".IF (b == 1)",
               "* Comment 3b1",
               ".ELSE",
               "* Comment 3belse",
               ".ENDIF",
               ".ELSEIF (a == 4)",
               "* Comment 4",
               ".ELSE",
               "* Comment 5",
               ".ENDIF",
               ".PARAM a = 3",
               ".PARAM b = 2",
               ".END");

            Assert.Equal(3, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.Equal("* Comment 3belse", ((CommentLine)netlist.Statements[0]).Text);

            Assert.True(netlist.Statements[1] is Control);
            Assert.True(netlist.Statements[2] is Control);
        }

        [Fact]
        public void NestedIfEleseTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false,
                true,
               "Nested netlist with if",
               ".IF (a > 0)",
               ".IF (a == 1)",
               "* Comment 1",
               ".ENDIF",
               ".ELSE",
               ".IF (a == 2)",
               "* Comment 2",
               ".ENDIF",
               ".ENDIF",
               ".PARAM a = 1",
               ".END");

            Assert.Equal(2, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.Equal("* Comment 1", ((CommentLine)netlist.Statements[0]).Text);

            Assert.True(netlist.Statements[1] is Control);
        }

        [Fact]
        public void BasicFalseTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false, 
                true,
               "Simplest netlist with if",
               ".IF (a == 0)",
               "* Comment 1",
               ".ENDIF",
               ".PARAM a = 1",
               ".END");

            Assert.Equal(1, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is Control);
        }

        [Fact]
        public void BasicTrueTest()
        {
            var netlist = ParseNetlistToPostProcessedModel(
                false,
                true,
               "Simplest netlist with if",
               ".IF (a == 1)",
               "* Comment 1",
               ".ENDIF",
               ".PARAM a = 1",
               ".END");

            Assert.Equal(2, netlist.Statements.Count);
            Assert.True(netlist.Statements[0] is CommentLine);
            Assert.True(netlist.Statements[1] is Control);
        }

        [Fact]
        public void SubcircuitTest()
        {
            var spiceSharpModel = ParseNetlist(
               "Subcircuit with if",
               ".SUBCKT resistor input output params: R=1",
               ".IF (R==1)",
               "R1 input output 1",
               ".ENDIF",
               ".IF (R==2)",
               "R1 input output 1",
               "R2 input output 2",
               ".ENDIF",
               ".ENDS resistor",
               "X 1 0 resistor R={r_global}",
               ".PARAM r_global = 2",
               ".END");

            Assert.Equal(2, spiceSharpModel.Circuit.Objects.Count);
        }
    }
}
