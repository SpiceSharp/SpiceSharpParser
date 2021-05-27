using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    public class CapacitorTests : BaseTests
    {
        [Fact]
        public void When_RCCircuitInOPSimulation_Expect_ShouldActLikeAnOpenCircuit()
        {
            var netlist = GetSpiceSharpModel(
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

            Assert.True(EqualsWithTol(new double[] { export }, references));
        }

        [Fact]
        public void When_RCCircuitInOPSimulationWithMultiply_Expect_ShouldActLikeAnOpenCircuit()
        {
            var netlist = GetSpiceSharpModel(
                "Lowpass RC circuit - The capacitor should act like an open circuit",
                "V1 IN 0 1.0",
                "R1 IN OUT 10e3",
                "C1 OUT 0 10e-6 m = 2",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            double export = RunOpSimulation(netlist, "V(OUT)");

            // Get references
            double[] references = { 1.0 };

            Assert.True(EqualsWithTol(new double[] { export }, references));
        }

        [Fact]
        public void When_RCCircuitInTranSimulation_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }


        [Fact]
        public void When_RCCircuitInTranSimulationWithMultiply_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * 2; // 0.000001 * 2
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 m = 2",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_RCCircuitInTranSimulationBehavioral_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 {V(IN2)}",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                "V2 IN2 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-12 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_RCCircuitInTranSimulationBehavioralWithMandN_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * 2.0 / 3.0;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 {V(IN2)} m = 2 n = 3",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                "V2 IN2 0 1e-6",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-12 10e-6",
                ".SAVE V(OUT)",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TemperatureInvariantCapacitor_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6; // 0.000001;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 TC=0,0",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".OPTIONS TEMP = 10",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TemperatureInvariantCapacitorWithMultiply_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * 2; // 0.000001 * 2;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 TC=0,0 m = 2",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".OPTIONS TEMP = 10",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TemperatureDependentCapacitor_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * (1 + 1.0 * 3.0 + 3.0 * 3.0 * 2.1); // 0.000001;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 TC=1.0,2.1",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".OPTIONS TEMP = 30",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TemperatureDependentCapacitorWithMuliply_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * (1 + 1.0 * 3.0 + 3.0 * 3.0 * 2.1) * 2; // 0.000001 * 2;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 TC=1.0,2.1 m = 2",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".OPTIONS TEMP = 30",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TemperatureDependentCapacitorOnlyTC1_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * (1 + 1.0 * 3.0); // 0.000001;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 TC=1.0",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".OPTIONS TEMP = 30",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }

        [Fact]
        public void When_TemperatureDependentCapacitorOnlyTC1WithMultiply_Expect_Reference()
        {
            double dcVoltage = 10;
            double resistorResistance = 10e3; // 10000;
            double capacitance = 1e-6 * (1 + 1.0 * 3.0) * 2; // 0.000001;
            double tau = resistorResistance * capacitance;

            var netlist = GetSpiceSharpModel(
                "Capacitor circuit - The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.",
                "C1 OUT 0 1e-6 TC=1.0 m = 2",
                "R1 IN OUT 10e3",
                "V1 IN 0 10",
                ".IC V(OUT)=0.0",
                ".TRAN 1e-8 10e-6",
                ".SAVE V(OUT)",
                ".OPTIONS TEMP = 30",
                ".END");

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));
            Assert.True(EqualsWithTol(exports, reference));
        }
    }
}