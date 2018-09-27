using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests
{
    public class ValueTest : BaseTest
    {
        [Fact]
        public void VoltageControlledVoltageSourceValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE = { V(1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceValueSimpleDependencyTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource1 3 0 VALUE = { V(1) + 2 }",
                "ESource 2 0 VALUE = { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(18, export);
        }

        [Fact]
        public void VoltageControlledVoltageSourceValueLoopTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "ESource 2 0 VALUE = { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(double.PositiveInfinity, export);
        }
        
        [Fact]
        public void CurrentControlledVoltageSourceValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(4, export);
        }

        [Fact]
        public void CurrentControlledVoltageSourceValueSimpleDependencyTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource1 3 0 VALUE = { I(I1) + 2 }",
                "HSource 2 0 VALUE = { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(18, export);
        }

        [Fact]
        public void CurrentControlledVoltageSourceValueLoopTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "HSource 2 0 VALUE = { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(double.PositiveInfinity, export);
        }

        [Fact]
        public void VoltageSourceValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "V2 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageSourceValueSimpleDependencyTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource1 3 0 VALUE = { I(I1) + 2 }",
                "V2 2 0 VALUE = { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(18, export);
        }

        [Fact]
        public void VoltageSourceValueLoopTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "V2 2 0 VALUE = { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(double.PositiveInfinity, export);
        }

        [Fact]
        public void VoltageSourceParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "V2 2 0 { I(I1) + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(4, export);
        }

        [Fact]
        public void VoltageSourceSimpleDependencyTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "I1 1 0 2",
                "HSource1 3 0 VALUE = { I(I1) + 2 }",
                "V2 2 0 { V(3,0) *  V(3,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(18, export);
        }

        [Fact]
        public void VoltageSourceLoopTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "V1 1 0 2",
                "V2 2 0 { V(2,0) *  V(2,0)  + 2 }",
                ".OP",
                ".SAVE V(2,0)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "V(2, 0)");

            Assert.Equal(double.PositiveInfinity, export);
        }

        [Fact]
        public void DCVoltageSweepTest()
        {
            var netlist = ParseNetlist(
                "DC Sweep - Voltage",
                "V1 in 0 0",
                "V2 out 0 {V(in, 0) + 100}",
                "R1 in 0 10",
                ".DC V1 -10 10 1e-3",
                ".SAVE V(out)",
                ".END");

            var exports = RunDCSimulation(netlist, "V(out)");

            // Create references
            Func<double, double>[] references = { sweep => sweep + 100 };
            EqualsWithTol(exports, references);
        }

        [Fact]
        public void TranComplexVoltageTestTest()
        {
            var netlist = ParseNetlist(
                "TRAN - Voltage TIME",
                "V1 in 0 {100 * sin(TIME * 10e6)}",
                "V2 out 0 {V(in, 0) * V(in, 0)}",
                "R1 in 0 10",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(out)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(out)");

            // Create references
            Func<double, double>[] references = { time => (Math.Sin(time * 10e6) * 100) * (Math.Sin(time * 10e6) * 100) };
            EqualsWithTol(exports, references);
        }

        [Fact]
        public void CurrentSourceValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Current source value circuit",
                "R1 2 0 100",
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
        public void CurrentSourceParsingTest()
        {
            var netlist = ParseNetlist(
                "Current source value circuit",
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
        public void VoltageControlledCurrentSourceValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
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
        public void CurrentControlledCurrentSourceValueParsingTest()
        {
            var netlist = ParseNetlist(
                "Diode circuit",
                "R1 1 0 100",
                "R2 1 0 200",
                "I1 1 0 2",
                "FSource 2 0 VALUE = { I(I1) + 2 }",
                ".OP",
                ".SAVE I(FSource)",
                ".END");

            Assert.NotNull(netlist);

            double export = RunOpSimulation(netlist, "I(FSource)");

            Assert.Equal(4, export);
        }
    }
}
