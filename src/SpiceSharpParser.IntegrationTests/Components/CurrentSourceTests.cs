using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class CurrentSourceTests : BaseTests
    {
        [Fact]
        public void PulseWithoutBracket_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Current source",
                "I1 1 0 PULSE 0 6 3.68us 41ns 41ns 3.256us 6.52us",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void PulseWithBracket_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Current source",
                "I1 1 0 PULSE(0V 6V 3.68us 41ns 41ns 3.256us 6.52us)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void SineWithBracket_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Current source",
                "I1 1 0 SINE(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void SineWithoutBracket_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Current source",
                "I1 1 0 SINE 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void SinWithBracket_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Current source",
                "I1 1 0 SIN(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void SinWithoutBracket_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Current source",
                "I1 1 0 SIN 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ACWithoutValue_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 AC",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ACWithDC_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 AC 1 DC 2",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void DCWithoutValue_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 DC",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ACPlusSin_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 AC 0",
                "+SIN 0 10 1000 0 0 0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ACPlusSine_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 AC 0",
                "+SINE 0 10 1000 0 0 0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ACPlusPulse_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 AC 0",
                "+PULSE 0V 5V 3.61us 41ns 41ns 4.255us 3.51us",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void ACPlusPwl_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 AC 0",
                "+Pwl -1.0 0 1.0 2.0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }

        [Fact]
        public void DCAndAC_Expect_NoException()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source",
                "I1 1 0 DC 1 AC 0",
                "+Pwl -1.0 0 1.0 2.0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".AC LIN 1000 1 1000",
                ".END");

            Assert.NotNull(netlist);
        }
    }
}