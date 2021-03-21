using Xunit;

namespace SpiceSharpParser.IntegrationTests.AnalogBehavioralModeling
{
    public class TableTests : BaseTests
    {
        [Fact]
        public void When_ParsingFirstFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} = (0,1) (1m,2) (2m,3)",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_MissingPoints_Expect_Exception()
        {
            var result = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)}",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.True(result.ValidationResult.HasWarning);
        }

        [Fact]
        public void When_ParsingSecondFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} (0,1) (1m,2) (2m,3)",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingThirdFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} ((0,1) (1m,2) (2m,3))",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingFourthFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} (0 1) (1m,2) (2m,3)",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingFifthFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} ((0 1) (1m,2))(2m,3))",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingSixthFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} ((0 1) (1m,2)((2m,3))",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingSeventhFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} ((0 1)) ((1m,2)(2m,3))",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingEightFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5m",
                "R1 1 0 10",
                "E12 2 1 TABLE {V(1,0)} ((0 1)) ((1m,2)) ((2m,3))",
                "R2 2 0 10",
                ".SAVE V(2,1)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(2,1)");
            Assert.Equal(2.5, export);
        }

        [Fact]
        public void When_ParsingAdvancedExpression_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "TABLE circuit",
                "V1 1 0 1.5",
                "V2 2 1 2.5",
                "R1 3 2 10",
                "R2 3 0 10",
                "E12 3 2 TABLE {9 + (V(1,0) + V(2,0))} ((-10,-10) (10, 10))",
                ".SAVE V(3,2)",
                ".OP",
                ".END");

            Assert.NotNull(netlist);
            var export = RunOpSimulation(netlist, "V(3,2)");
            Assert.Equal(10, export);
        }
    }
}