using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Waveforms
{
    public class AMTests : BaseTests
    {
        [Theory]
        [InlineData(1.0, 0.0, 1.0, 5.0, 0.0, 0.0, 0.0)]
        [InlineData(1.0, -1.0, 1.0, 2.5, 0.2, 2.0, 1.0)]
        public void Test01(double va, double vo, double mf, double fc, double td, double phasec, double phases)
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - AM waveform",
                $"V1 a 0 AM({va} {vo} {mf} {fc} {td} {phasec} {phases})",
                ".SAVE V(a)",
                ".TRAN 1e-6 1",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(a)");
            Func<double, double> reference = time => time < td ? 0.0 : va * (vo + Math.Sin(2.0 * Math.PI * mf * (time - td) + phases * Math.PI / 180.0)) *
                        Math.Sin(2.0 * Math.PI * fc * (time - td) + phasec * Math.PI / 180.0);

            Assert.True(EqualsWithTol(exports, reference));
        }

        [Theory]
        [InlineData(1.0, 0.0, 1.0, 5.0, 0.0, 0.0, 0.0)]
        [InlineData(1.0, -1.0, 1.0, 2.5, 0.2, 2.0, 1.0)]
        public void Test02(double va, double vo, double mf, double fc, double td, double phasec, double phases)
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - AM waveform",
                $"V1 a 0 AM({va}, {vo}, {mf}, {fc}, {td}, {phasec}, {phases})",
                ".SAVE V(a)",
                ".TRAN 1e-6 1",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(a)");
            Func<double, double> reference = time => time < td ? 0.0 : va * (vo + Math.Sin(2.0 * Math.PI * mf * (time - td) + phases * Math.PI / 180.0)) *
                        Math.Sin(2.0 * Math.PI * fc * (time - td) + phasec * Math.PI / 180.0);
            Assert.True(EqualsWithTol(exports, reference));
        }
    }
}
