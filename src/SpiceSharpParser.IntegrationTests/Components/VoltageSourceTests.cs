using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class VoltageSourceTests : BaseTests
    {
        [Fact]
        public void PulseWithoutBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 PULSE 0V 6V 3.68us 41ns 41ns 3.256us 6.52us",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void PulseWithBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 PULSE(0V 6V 3.68us 41ns 41ns 3.256us 6.52us)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");
            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void SineWithBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 SINE(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void SineWithoutBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 SINE 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void SinWithBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 SIN(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void SinWithoutBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 SIN 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void PwlWithBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl(0 1 1 2 2 3)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void PwlWithoutBracket_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl 0 1 1 2 2 3",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void PwlWithoutBracketWithCommas_NoException()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl 0 1, 1 2, 2 3",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            RunTransientSimulation(netlist, "V(1,0)");
        }

        [Fact]
        public void Pwl_OnePoint_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl(0 2)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(1,0)");
            Assert.True(exports.All(export => export.Item2 == 2.0));
        }

        [Fact]
        public void Pwl_TwoPoints_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl(0 0 1.0 2.0)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithToWithoutAssert(export.Item2, (export.Item1 < 1.0 ? 2.0 * export.Item1 : 2.0))));
        }

        [Fact]
        public void Pwl_TwoPointsWithCommas_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl 0 0, 1.0 2.0",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithToWithoutAssert(export.Item2, (export.Item1 < 1.0 ? 2.0 * export.Item1 : 2.0))));
        }

        [Fact]
        public void Pwl_MinusTimePoint_Expect_Reference()
        {
            var netlist = ParseNetlist(
                "Voltage source",
                "V1 1 0 Pwl(-1.0 0 1.0 2.0)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 0.1 1.5",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(1,0)");
            Assert.True(exports.All(export => EqualsWithToWithoutAssert(export.Item2, (export.Item1 < 1.0 ? 1.0 + export.Item1 : 2.0))));
        }
    }
}
