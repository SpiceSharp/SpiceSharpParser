using SpiceSharp.Simulations;
using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Waveforms
{
    public class PulseTests : BaseTests
    {
        [Fact]
        public void Test01()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - Pulse waveform",
                $"V1 a 0 PULSE(0, 1, 0.2, 0.1, 0.1, 0.4, 1.0)",
                ".SAVE V(a)",
                ".TRAN 0.1 1.2",
                ".END");

            Assert.NotNull(netlist);

            var simulation = netlist.Simulations.First(s => s is Transient);
            bool riseHit = false, risenHit = false, fallHit = false, fallenHit = false;

            simulation.ExportSimulationData += (sender, args) =>
            {
                if (Math.Abs(args.Time - 0.2) < 1e-12)
                    riseHit = true;
                if (Math.Abs(args.Time - 0.3) < 1e-12)
                    risenHit = true;
                if (Math.Abs(args.Time - 0.7) < 1e-12)
                    fallHit = true;
                if (Math.Abs(args.Time - 0.8) < 1e-12)
                    fallenHit = true;
            };

            simulation.Run(netlist.Circuit);

            Assert.True(riseHit);
            Assert.True(risenHit);
            Assert.True(fallHit);
            Assert.True(fallenHit);
        }

        [Fact]
        public void Test02()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - Pulse waveform",
                $"V1 a 0 PULSE(0 1 0.2 0.1 0.1 0.4 1.0)",
                ".SAVE V(a)",
                ".TRAN 0.1 1.2",
                ".END");

            Assert.NotNull(netlist);

            var simulation = netlist.Simulations.First(s => s is Transient);
            bool riseHit = false, risenHit = false, fallHit = false, fallenHit = false;

            simulation.ExportSimulationData += (sender, args) =>
            {
                if (Math.Abs(args.Time - 0.2) < 1e-12)
                    riseHit = true;
                if (Math.Abs(args.Time - 0.3) < 1e-12)
                    risenHit = true;
                if (Math.Abs(args.Time - 0.7) < 1e-12)
                    fallHit = true;
                if (Math.Abs(args.Time - 0.8) < 1e-12)
                    fallenHit = true;
            };

            simulation.Run(netlist.Circuit);

            Assert.True(riseHit);
            Assert.True(risenHit);
            Assert.True(fallHit);
            Assert.True(fallenHit);
        }
    }
}
