using SpiceSharp.Simulations;
using System.Linq;
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
            var raw = (Transient)simulation;
            var wasHit1 = false;
            var wasHit2 = false;

            simulation.EventExportData += (sender, args) =>
            {
                if (raw.Time == 1.111)
                {
                    wasHit1 = true;
                }

                if (raw.Time == 3.34)
                {
                    wasHit2 = true;
                }

                Assert.True(EqualsWithTol(2.0, simulation.GetVoltage("a")));
            };

            var codes = simulation.Run(netlist.Circuit);
            var withEvents = simulation.InvokeEvents(codes);

            withEvents.ToArray(); //eval

            Assert.True(wasHit1);
            Assert.True(wasHit2);
        }
    }
}
