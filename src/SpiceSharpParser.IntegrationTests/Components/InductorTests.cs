using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class InductorTests : BaseTests
    {
        [Fact]
        public void When_InductorIC_Expect_Reference()
        {
            double L = 1.0, R = 1.0e3, i0 = 1.0;
            var tau = L / R;

            var netlist = GetSpiceSharpModel(
                "Inductor circuit",
                "V1 in 0 0.0",
                $"L1 in out {L} ic = {i0}",
                $"R1 out 0 {R}",
                ".TRAN 1e-6 1e-3 uic",
                ".IC V(out)=0.0",
                ".SAVE V(out)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(out)");
            Func<double, double> references = t => t > 0 ? i0 * R * Math.Exp(-t / tau) : 0.0;
            Assert.True(EqualsWithTol(exports, references));
        }

        [Fact]
        public void When_InductorICWithMultiply_Expect_Reference()
        {
            double L = 1.0, R = 1.0e3, i0 = 1.0;
            var tau = L / 2.0 / R;

            var netlist = GetSpiceSharpModel(
                "Inductor circuit",
                "V1 in 0 0.0",
                $"L1 in out {L} ic = {i0} m = 2",
                $"R1 out 0 {R}",
                ".TRAN 1e-6 1e-3 uic",
                ".IC V(out)=0.0",
                ".SAVE V(out)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(out)");
            Func<double, double> references = t => t > 0 ? i0 * R * Math.Exp(-t / tau) : 0.0;
            Assert.True(EqualsWithTol(exports, references));
        }
    }
}
