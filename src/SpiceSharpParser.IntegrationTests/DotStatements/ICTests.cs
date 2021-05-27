using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class ICTests : BaseTests
    {
        [Fact]
        public void When_RCCircuitInTranSimulation_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var model = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(model, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_InsideSubcircuit_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var model = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "X1 IN OUT comp",
                ".SUBCKT capacitor in out",
                "C1 out in 1e-6",
                ".ENDS",
                ".SUBCKT comp c_in c_out",
                "X1 c_out 0 capacitor",
                "R1 c_in c_out 10e3",
                "V1 c_in 0 10",
                ".IC V(X1.out)=0.0",
                ".ENDS",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(model, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));

            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_InsideSubcircuit2_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var model = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "X1 IN OUT comp",
                ".SUBCKT capacitor in out",
                "C1 out in 1e-6",
                ".ENDS",
                ".SUBCKT comp c_in c_out",
                "X1 c_out 0 capacitor",
                "R1 c_in c_out 10e3",
                "V1 c_in 0 10",
                ".IC V(c_out)=0.0",
                ".ENDS",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(model, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));

            Assert.True(EqualsWithTol(exports, reference));
        }
    }
}