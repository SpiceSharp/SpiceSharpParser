using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class CurrentSourceTests : BaseTests
    {
        //[Fact]
        public void PulseWithoutBracket()
        {
            var netlist = ParseNetlist(
                "Current source",
                "I1 1 0 PULSE 0 6 3.68us 41ns 41ns 3.256us 6.52us",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            var export = RunTransientSimulation(netlist, "V(1,0)");
            Assert.NotNull(netlist);
        }

        //[Fact]
        public void PulseWithBracket()
        {
            var netlist = ParseNetlist(
                "Current source",
                "I1 1 0 PULSE(0V 6V 3.68us 41ns 41ns 3.256us 6.52us)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            var export = RunTransientSimulation(netlist, "V(1,0)");
            Assert.NotNull(netlist);
        }

        [Fact]
        public void SineWithBracket()
        {
            var netlist = ParseNetlist(
                "Current source",
                "I1 1 0 SINE(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            var export = RunTransientSimulation(netlist, "V(1,0)");
            Assert.NotNull(netlist);
        }

        [Fact]
        public void SineWithoutBracket()
        {
            var netlist = ParseNetlist(
                "Current source",
                "I1 1 0 SINE 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            var export = RunTransientSimulation(netlist, "V(1,0)");
            Assert.NotNull(netlist);
        }

        [Fact]
        public void SinWithBracket()
        {
            var netlist = ParseNetlist(
                "Current source",
                "I1 1 0 SIN(0 5 50 0 0 90)",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            var export = RunTransientSimulation(netlist, "V(1,0)");
            Assert.NotNull(netlist);
        }

        [Fact]
        public void SinWithoutBracket()
        {
            var netlist = ParseNetlist(
                "Current source",
                "I1 1 0 SIN 0 5 50 0 0 90",
                "R1 1 0 10",
                ".SAVE V(1,0)",
                ".TRAN 1e-8 1e-5",
                ".END");

            var export = RunTransientSimulation(netlist, "V(1,0)");
            Assert.NotNull(netlist);
        }
    }
}
