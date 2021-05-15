using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Waveforms
{
    public class SFFMTests : BaseTests
    {
        [Theory]
        [InlineData(1.0, 1.0, 1.0, 5.0, 0.0, 0.0, 0.0)]
        [InlineData(1.0, 10.0, 1.0, 2.5, 0.2, 2.0, 1.0)]
        public void Test01(double vo, double va, double fc, double mdi, double fs, double phasec, double phases)
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - SFFM waveform",
                $"V1 a 0 SFFM({vo} {va} {fc} {mdi} {fs} {phasec} {phases})",
                ".SAVE V(a)",
                ".TRAN 1e-6 1",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(a)");
            Func<double, double> reference = time => vo + va * Math.Sin(2.0 * Math.PI * fc * time + phasec * Math.PI / 180.0 +
                    mdi * Math.Sin(2.0 * Math.PI * fs * time + phases * Math.PI / 180.0));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Theory]
        [InlineData(1.0, 1.0, 1.0, 5.0, 0.0, 0.0, 0.0)]
        [InlineData(1.0, 10.0, 1.0, 2.5, 0.2, 2.0, 1.0)]
        public void Test02(double vo, double va, double fc, double mdi, double fs, double phasec, double phases)
        {
            var netlist = GetSpiceSharpModel(
                "Voltage source - SFFM waveform",
                $"V1 a 0 SFFM({vo}, {va}, {fc}, {mdi}, {fs}, {phasec}, {phases})",
                ".SAVE V(a)",
                ".TRAN 1e-6 1",
                ".END");

            Assert.NotNull(netlist);
            var exports = RunTransientSimulation(netlist, "V(a)");
            Func<double, double> reference = time => vo + va * Math.Sin(2.0 * Math.PI * fc * time + phasec * Math.PI / 180.0 +
                    mdi * Math.Sin(2.0 * Math.PI * fs * time + phases * Math.PI / 180.0));
            Assert.True(EqualsWithTol(exports, reference));
        }
    }
}
