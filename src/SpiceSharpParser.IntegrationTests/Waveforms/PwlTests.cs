using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Waveforms
{
    public class PwlTests : BaseTests
    {
        [Fact]
        public void Test01()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - Pwl waveform",
                $"V1 a 0 Pwl(1.111 2.0 3.34 2.0)",
                ".SAVE V(a)",
                ".TRAN 1.3e-6 10.0",
                ".END");

            Assert.NotNull(netlist);

            var simulation = netlist.Simulations.First(s => s is Transient);

            var wasHit1 = false;
            var wasHit2 = false;

            simulation.ExportSimulationData += (sender, args) =>
            {
                if (args.Time == 1.111)
                {
                    wasHit1 = true;
                }

                if (args.Time == 3.34)
                {
                    wasHit2 = true;
                }

                Assert.True(EqualsWithTol(2.0, args.GetVoltage("a")));
            };

            simulation.Run(netlist.Circuit);

            Assert.True(wasHit1);
            Assert.True(wasHit2);
        }
    }
}
