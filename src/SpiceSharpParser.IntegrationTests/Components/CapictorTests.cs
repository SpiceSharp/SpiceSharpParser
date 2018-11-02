using SpiceSharp.Simulations;
using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class CapacitorTests : BaseTests
    {
        [Fact]
        public void ShouldActLikeAnOpenCircuit()
        {
            var netlist = ParseNetlist(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 1.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { 1.0 };

            EqualsWithTol(new double[] { export }, references);
        }

        [Fact]
        public void ExponentialConvergingToDcVoltage()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var netlist = ParseNetlist(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double>[] references = { t => dcVoltage * (1.0 - Math.Exp(-t / tau)) };
            EqualsWithTol(exports, references);
        }
    }
}
