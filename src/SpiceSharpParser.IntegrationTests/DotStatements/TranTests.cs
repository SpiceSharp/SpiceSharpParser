using System;
using System.Linq;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class TranTests : BaseTests
    {
        [Fact]
        public void When_TranHasUIC_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var model = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 ic=0.0",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".TRAN 1e-8 1e-5 uic",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(model, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TranHasTStart_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var model = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 IC=0.0",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".TRAN 1e-8 1e-5 0.5*1e-5 1e-6 UIC",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(model, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));

            Assert.False(exports.Any(export => export.Item1 < 0.5 * 1e-5), "There shouldn't be a export at that time");
            Assert.True(EqualsWithTol(exports, reference));
        }
    }
}