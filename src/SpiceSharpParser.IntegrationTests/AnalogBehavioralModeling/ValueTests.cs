using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.AnalogBehavioralModeling
{
    public class ValueTests : BaseTests
    {
        [Fact]
        public void When_VoltageControlledVoltageSourceCasing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 1",
                "V2 2 0 2",
                "V3 3 0 3",
                "V4 4 0 4",
                "V5 5 0 5",
                "X1 3 6 4 COMP1",
                ".SUBCKT COMP1 4 5 2",
                "ESource 4 5 VALUE = { v(2) + 2 }",
                ".ENDS",
                ".OP",
                ".SAVE V(3,6)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(3,6)");
            Assert.Equal(6, export);
        }

        [Fact]
        public void When_VoltageControlledVoltageSourceIsInsideSubckt_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 1",
                "V2 2 0 2",
                "V3 3 0 3",
                "V4 4 0 4",
                "V5 5 0 5",
                "X1 3 6 4 COMP1",
                ".SUBCKT COMP1 4 5 2",
                "ESource 4 5 VALUE = { V(2) + 2 }",
                ".ENDS",
                ".OP",
                ".SAVE V(3,6)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(3,6)");
            Assert.Equal(6, export);
        }

        [Fact]
        public void When_VoltageControlledVoltageSourcePower_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 1",
                "V2 2 0 2",
                "V3 3 0 3",
                "V4 4 0 4",
                "V5 5 0 5",
                "X1 3 6 4 COMP1",
                ".SUBCKT COMP1 4 5 2",
                "ESource 4 5 VALUE = { pow(V(2), 2) + 2 }",
                ".ENDS",
                ".OP",
                ".SAVE V(3,6)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(3,6)");
            Assert.Equal(18, export);
        }

        [Fact]
        public void When_TwoVoltageControlledVoltageSourceInSubcktSameVoltage_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 1",
                "V2 2 1 2",
                "V3 3 1 3",
                "V4 4 1 4",
                "X1 3 6 3 1 COMP1",
                "X2 3 7 4 1 COMP2",
                ".SUBCKT COMP1 4 5 2 1",
                "ESource 4 5 VALUE = { V(2,1) + 2 }",
                ".ENDS",
                ".SUBCKT COMP2 4 5 2 1",
                "ESource 4 5 VALUE = { V(2,1) + 2 }",
                ".ENDS",
                ".OP",
                ".SAVE V(2,1) V(3,7)",
                ".END");

            Assert.NotNull(netlist);
            double[] exports = RunOpSimulation(netlist, "V(2,1)", "V(3,7)");
            Assert.Equal(2, exports[0]);
            Assert.Equal(6, exports[1]);
        }

        [Fact]
        public void When_VoltageControlledVoltageSourceValueValue_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE = { V(1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageControlledVoltageSourceValueWithoutEqual_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE { V(1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageControlledVoltageSourceValueSimpleDependency_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource1 3 0 VALUE = { V(1) + 2 }",
                "ESource 2 0 VALUE = { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(18, export);
        }

        [Fact]
        public void When_VoltageControlledVoltageSourceValueLoop_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE = { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.Throws<SpiceSharp.SpiceSharpException>(() => RunOpSimulation(netlist, "V(2,0)"));
        }

        [Fact]
        public void When_CurrentControlledVoltageSourceValueParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_CurrentControlledVoltageSourceValueParsingWithoutEqual_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource 2 0 VALUE { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_CurrentControlledVoltageSourceValueSimpleDependency_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource1 3 0 VALUE = { I(I1) + 2 }",
                "HSource 2 0 VALUE = { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(18, export);
        }

        [Fact]
        public void When_CurrentControlledVoltageSourceValueLoop_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "HSource 2 0 VALUE = { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.Throws<SpiceSharp.SpiceSharpException>(() => RunOpSimulation(netlist, "V(2,0)"));
        }

        [Fact]
        public void When_VoltageSourceValueParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "V2 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageSourceValueSimpleDependency_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource1 3 0 VALUE = { I(I1) + 2 }",
                "V2 2 0 VALUE = { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(18, export);
        }

        [Fact]
        public void When_VoltageSourceValueLoop_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "V2 2 0 VALUE = { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.Throws<SpiceSharp.SpiceSharpException>(() => RunOpSimulation(netlist, "V(2,0)"));
        }

        [Fact]
        public void When_VoltageSourceParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "V2 2 0 { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageSourceParsingDifferentFormat_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "V2 2 0 VALUE { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageSourceSimpleDependency_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource1 3 0 VALUE = { I(I1) + 2 }",
                "V2 2 0 { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "V(2,0)");
            Assert.Equal(18, export);
        }

        [Fact]
        public void When_VoltageSourceLoop_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "V2 2 0 { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);
            Assert.Throws<SpiceSharp.SpiceSharpException>(() => RunOpSimulation(netlist, "V(2,0)"));
        }

        [Fact]
        public void When_DCVoltageSweep_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value - DC Sweep - Voltage",
                "V1 in 0 0",
                "V2 out 0 {V(in, 0) + 100}",
                "R1 in 0 10",
                ".DC V1 -10 10 1e-3",
                ".SAVE V(out)",
                ".END");

            var exports = RunDCSimulation(netlist, "V(out)");

            // Get references
            Func<double, double> reference = sweep => sweep + 100;
            EqualsWithTol(exports, reference);
        }

        [Fact]
        public void When_TranComplexVoltage_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value - TRAN - Voltage TIME",
                "V1 in 0 {100 * sin(TIME * 10e6)}",
                "V2 out 0 {V(in, 0) * V(in, 0)}",
                "R1 in 0 10",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(out)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(out)");

            // Get references
            Func<double, double> reference = time => (Math.Sin(time * 10e6) * 100) * (Math.Sin(time * 10e6) * 100);
            EqualsWithTol(exports, reference);
        }

        [Fact]
        public void When_CurrentSourceValueParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Current source value circuit",
                "R1 2 1 100",
                "R2 1 0 1000",
                "I1 1 0 2",
                "I2 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE I(I2)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(I2)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_CurrentSourceValueWithoutEqualParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Current source value circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "R2 2 0 100",
                "I2 2 0 VALUE { I(I1) + 2 }",
                ".OP",
                ".SAVE I(I2)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(I2)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_CurrentSourceParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Current source value circuit",
                "R0 1 0 10",
                "R1 2 0 100",
                "I1 1 0 2",
                "I2 2 0 { I(I1) + 2 }",
                ".OP",
                ".SAVE I(I2)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(I2)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageControlledCurrentSourceValueParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 VALUE = { V(2) + 2 }",
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_VoltageControlledCurrentSourceValueParsingWithoutEqual_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "V1 2 0 2",
                "GSource 1 0 VALUE { V(2) + 2 }",
                ".OP",
                ".SAVE I(GSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(GSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_CurrentControlledCurrentSourceValueParsing_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "R2 2 0 200",
                "I1 1 0 2",
                "FSource 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE I(FSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(FSource)");
            Assert.Equal(4, export);
        }

        [Fact]
        public void When_CurrentControlledCurrentSourceValueParsingWithoutEqual_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Value test circuit",
                "R1 1 0 100",
                "R2 2 0 200",
                "I1 1 0 2",
                "FSource 2 0 VALUE { I(I1) + 2 }",
                ".OP",
                ".SAVE I(FSource)",
                ".END");

            Assert.NotNull(netlist);
            double export = RunOpSimulation(netlist, "I(FSource)");
            Assert.Equal(4, export);
        }
    }
}