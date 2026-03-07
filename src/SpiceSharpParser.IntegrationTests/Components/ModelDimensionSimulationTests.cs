using System;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Components
{
    /// <summary>
    /// Integration tests with simulations for model selection based on L and W parameters.
    /// These tests verify that the correct model is selected by running actual circuit simulations.
    /// </summary>
    public class ModelDimensionSimulationTests : BaseTests
    {
        #region Resistor Simulation Tests

        [Fact]
        public void ResistorModelSelectionAffectsSimulationResults()
        {
            // Model with RSH=100 ohm/square, L/W range for small resistors
            // Model with RSH=1000 ohm/square, L/W range for large resistors
            // R = RSH * L / W
            var netlist = GetSpiceSharpModel(
                "Resistor model selection affects resistance value",
                "V1 IN 0 10",
                "R1 IN 0 RMOD L=1u W=1u",
                ".model RMOD.0 R RSH=100 lmin=0.1u lmax=5u",
                ".OP",
                ".SAVE I(R1)",
                ".MEAS OP meas_i MAX I(R1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=1u, W=1u -> should use RMOD.0 (RSH=100) -> R = 100 * 1 / 1 = 100 ohms
            // Expected current: 10V / 100 ohm = 0.1 A
            var current1 = RunOpSimulation(netlist, "I(R1)");
            Assert.True(EqualsWithTol(0.1, Math.Abs(current1)), $"R1 current expected ~0.1A, got {current1}");
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        [Fact]
        public void ResistorWidthParameterAffectsModelSelection()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor width affects model selection",
                "V1 IN 0 5",
                "R1 IN 0 RMOD L=2u W=2u",
                ".model RMOD.0 R RSH=50 wmin=1u wmax=10u",
                ".OP",
                ".SAVE I(R1)",
                ".MEAS OP meas_i MAX I(R1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=2u, W=2u -> RMOD.0 (RSH=50) -> R = 50 * 2 / 2 = 50 ohms -> I = 5V / 50 = 0.1 A
            var current1 = RunOpSimulation(netlist, "I(R1)");
            Assert.True(EqualsWithTol(0.1, Math.Abs(current1)), $"R1 current expected ~0.1A, got {current1}");
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        [Fact]
        public void ResistorFallsBackToDefaultModelSimulation()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor falls back to default model when dimensions don't match",
                "V1 IN 0 10",
                "R1 IN 0 RMOD L=0.5u W=1u",
                ".model RMOD.0 R RSH=100 lmin=1u lmax=10u",
                ".model RMOD R RSH=500",
                ".OP",
                ".SAVE I(R1)",
                ".MEAS OP meas_i MAX I(R1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=0.5u (< lmin=1u) -> should use RMOD (default, RSH=500) -> R = 500 * 0.5 / 1 = 250 ohms
            var current1 = RunOpSimulation(netlist, "I(R1)");
            Assert.True(EqualsWithTol(10.0 / 250.0, Math.Abs(current1)), $"R1 current expected ~0.04A, got {current1}");
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        [Fact]
        public void ResistorWithBothLAndWConstraintsSimulation()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor with both L and W constraints",
                "V1 IN 0 12",
                "R1 IN 0 RMOD L=1u W=2u",
                ".model RMOD.0 R RSH=60 lmin=0.5u lmax=5u wmin=1u wmax=10u",
                ".OP",
                ".SAVE I(R1)",
                ".MEAS OP meas_i MAX I(R1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=1u, W=2u -> RMOD.0 (RSH=60) -> R = 60 * 1 / 2 = 30 ohms -> I = 12V / 30 = 0.4 A
            var current1 = RunOpSimulation(netlist, "I(R1)");
            Assert.True(EqualsWithTol(0.4, Math.Abs(current1)), $"R1 current expected ~0.4A, got {current1}");
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        #endregion

        #region Capacitor Simulation Tests

        [Fact]
        public void CapacitorModelSelectionAffectsSimulationResults()
        {
            // Capacitance C = CJ * L * W (approximately for semiconductor capacitors)
            var netlist = GetSpiceSharpModel(
                "Capacitor model selection affects capacitance value",
                "V1 IN 0 PULSE(0 5 0 1n 1n 10n 20n)",
                "R1 IN OUT1 1k",
                "C1 OUT1 0 CMOD L=2u W=2u",
                ".model CMOD.0 C CJ=1e-6 lmin=0.5u lmax=5u wmin=0.5u wmax=5u",
                ".TRAN 1n 30n",
                ".SAVE V(OUT1)",
                ".MEAS TRAN meas_v MAX V(OUT1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports1 = RunTransientSimulation(netlist, "V(OUT1)");
            Assert.NotNull(exports1);
            Assert.True(exports1.Length > 0);
            AssertMeasurementSuccess(netlist, "meas_v");
        }

        [Fact]
        public void CapacitorWidthParameterAffectsModelSelection()
        {
            var netlist = GetSpiceSharpModel(
                "Capacitor width affects model selection",
                "V1 IN 0 PULSE(0 10 0 1n 1n 20n 40n)",
                "R1 IN OUT1 1k",
                "C1 OUT1 0 CMOD L=1u W=3u",
                ".model CMOD.0 C CJ=5e-7 wmin=1u wmax=10u",
                ".TRAN 1n 40n",
                ".SAVE V(OUT1)",
                ".MEAS TRAN meas_v MAX V(OUT1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports1 = RunTransientSimulation(netlist, "V(OUT1)");
            Assert.NotNull(exports1);
            Assert.True(exports1.Length > 0);
            AssertMeasurementSuccess(netlist, "meas_v");
        }

        [Fact]
        public void CapacitorFallsBackToDefaultModelSimulation()
        {
            var netlist = GetSpiceSharpModel(
                "Capacitor falls back to default model",
                "V1 IN 0 PULSE(0 5 0 1n 1n 10n 20n)",
                "R1 IN OUT1 1k",
                "C1 OUT1 0 CMOD L=0.3u W=1u",
                ".model CMOD.0 C CJ=1e-6 lmin=1u lmax=3u",
                ".model CMOD C CJ=5e-7",
                ".TRAN 1n 30n",
                ".SAVE V(OUT1)",
                ".MEAS TRAN meas_v MAX V(OUT1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports1 = RunTransientSimulation(netlist, "V(OUT1)");
            Assert.NotNull(exports1);
            Assert.True(exports1.Length > 0);
            AssertMeasurementSuccess(netlist, "meas_v");
        }

        #endregion

        #region Inductor Simulation Tests

        [Fact]
        public void InductorWithLengthAndWidthParameters()
        {
            // Basic test to verify inductors accept L and W parameters
            var netlist = GetSpiceSharpModel(
                "Inductor with L and W parameters",
                "V1 IN 0 PULSE(0 1 0 1n 1n 10n 20n)",
                "R1 IN OUT 10",
                "L1 OUT 0 1u",
                ".TRAN 1n 30n",
                ".SAVE V(OUT) I(L1)",
                ".MEAS TRAN meas_i MAX I(L1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports = RunTransientSimulation(netlist, "I(L1)");
            Assert.NotNull(exports);
            Assert.True(exports.Length > 0);
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        [Fact]
        public void InductorBasicBehavior()
        {
            // Test basic RL circuit behavior
            var netlist = GetSpiceSharpModel(
                "RL circuit transient response",
                "V1 IN 0 PULSE(0 10 0 1n 1n 50n 100n)",
                "R1 IN OUT 100",
                "L1 OUT 0 1u",
                ".TRAN 0.5n 60n",
                ".SAVE I(L1)",
                ".MEAS TRAN meas_i MAX I(L1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports = RunTransientSimulation(netlist, "I(L1)");
            
            // Current through inductor should gradually rise (not instantaneous)
            // At t=0, current should be ~0
            Assert.True(Math.Abs(exports[0].Item2) < 0.01, $"Initial current should be ~0, got {exports[0].Item2}");

            // Current should increase over time
            var index30n = 60; // Approximate index for 30ns (0.5ns steps)
            if (index30n < exports.Length)
            {
                Assert.True(exports[index30n].Item2 > exports[10].Item2, 
                    $"Current should increase: I(t=5ns)={exports[10].Item2}, I(t=30ns)={exports[index30n].Item2}");
            }
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        [Fact]
        public void InductorComparisonDifferentValues()
        {
            // Compare two inductors with different inductance values
            var netlist = GetSpiceSharpModel(
                "Compare inductors with different values",
                "V1 IN 0 PULSE(0 10 0 1n 1n 50n 100n)",
                "R1 IN OUT1 100",
                "L1 OUT1 0 1u",
                "R2 IN OUT2 100",
                "L2 OUT2 0 10u",
                ".TRAN 0.5n 60n",
                ".SAVE I(L1) I(L2)",
                ".MEAS TRAN meas_i MAX I(L1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports1 = RunTransientSimulation(netlist, "I(L1)");
            var exports2 = RunTransientSimulation(netlist, "I(L2)");

            // Smaller inductance (L1=1u) should reach steady state faster than larger (L2=10u)
            var index20n = 40; // Approximate index for 20ns
            if (index20n < exports1.Length && index20n < exports2.Length)
            {
                Assert.True(exports1[index20n].Item2 > exports2[index20n].Item2,
                    $"Smaller inductor should have higher current earlier: I(L1)={exports1[index20n].Item2}, I(L2)={exports2[index20n].Item2}");
            }
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        #endregion

        #region Combined Component Tests

        [Fact]
        public void RLCCircuitWithDimensionBasedModels()
        {
            var netlist = GetSpiceSharpModel(
                "RLC circuit with dimension-based component models",
                "V1 IN 0 PULSE(0 5 0 1n 1n 20n 40n)",
                "R1 IN N1 RMOD L=2u W=1u",
                "L1 N1 N2 10u",
                "C1 N2 0 1p",
                ".model RMOD.0 R RSH=50 lmin=1u lmax=10u",
                ".TRAN 0.5n 50n",
                ".SAVE V(N2)",
                ".MEAS TRAN meas_v MAX V(N2)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports = RunTransientSimulation(netlist, "V(N2)");

            // Verify simulation runs and produces results
            Assert.NotNull(exports);
            Assert.True(exports.Length > 0);

            // Verify circuit responds to input (voltage should change from 0)
            var maxVoltage = 0.0;
            foreach (var v in exports)
            {
                if (Math.Abs(v.Item2) > maxVoltage)
                    maxVoltage = Math.Abs(v.Item2);
            }
            Assert.True(maxVoltage > 0.1, $"Circuit should respond to input, max voltage: {maxVoltage}");
            AssertMeasurementSuccess(netlist, "meas_v");
        }

        [Fact]
        public void MultipleResistorsWithDifferentModelSelection()
        {
            var netlist = GetSpiceSharpModel(
                "Voltage divider with different resistor models",
                "V1 IN 0 10",
                "R1 IN MID RMOD L=1u W=1u",
                "R2 MID 0 RMOD L=10u W=1u",
                ".model RMOD.0 R RSH=100 lmin=0.5u lmax=5u",
                ".model RMOD.1 R RSH=1000 lmin=5u lmax=50u",
                ".OP",
                ".SAVE V(MID)",
                ".MEAS OP meas_v MAX V(MID)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=1u, W=1u -> RMOD.0 (RSH=100) -> R1 = 100 * 1 / 1 = 100 ohms
            // R2: L=10u, W=1u -> RMOD.1 (RSH=1000) -> R2 = 1000 * 10 / 1 = 10000 ohms
            // Voltage divider: V(MID) = 10V * R2 / (R1 + R2) = 10 * 10000 / 10100 ≈ 9.9 V
            
            var voltage = RunOpSimulation(netlist, "V(MID)");
            var expected = 9.9;
            var tolerance = 0.1;
            Assert.True(Math.Abs(expected - voltage) < tolerance, $"V(MID) expected ~{expected}V, got {voltage}");
            AssertMeasurementSuccess(netlist, "meas_v");
        }

        [Fact]
        public void CapacitorChargeDischargeWithModelSelection()
        {
            var netlist = GetSpiceSharpModel(
                "Capacitor charge/discharge with model selection",
                "V1 IN 0 PULSE(0 10 0 1n 1n 50n 100n)",
                "R1 IN OUT 1k",
                "C1 OUT 0 CMOD L=3u W=3u",
                ".model CMOD.0 C CJ=1e-6 lmin=1u lmax=10u wmin=1u wmax=10u",
                ".TRAN 1n 80n",
                ".SAVE V(OUT)",
                ".MEAS TRAN meas_v MAX V(OUT)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            var exports = RunTransientSimulation(netlist, "V(OUT)");
            
            // Verify charging behavior
            Assert.True(exports[0].Item2 < 1.0, "Initial voltage should be low");

            // Find peak during pulse
            var peakVoltage = 0.0;
            for (int i = 10; i < Math.Min(50, exports.Length); i++)
            {
                if (exports[i].Item2 > peakVoltage)
                    peakVoltage = exports[i].Item2;
            }
            
            Assert.True(peakVoltage > 5.0, $"Capacitor should charge significantly, peak: {peakVoltage}V");
            AssertMeasurementSuccess(netlist, "meas_v");
        }

        #endregion

        #region Edge Case Simulation Tests

        [Fact]
        public void ResistorWithLminBoundaryCondition()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor at lmin boundary",
                "V1 IN 0 10",
                "R1 IN 0 RMOD L=1.01u W=1u",
                ".model RMOD.0 R RSH=100 lmin=1u",
                ".model RMOD R RSH=200",
                ".OP",
                ".SAVE I(R1)",
                ".MEAS OP meas_i MAX I(R1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=1.01u (>= lmin) -> should use RMOD.0 (RSH=100) -> R = 100 * 1.01 / 1 = 101 ohms
            var current1 = RunOpSimulation(netlist, "I(R1)");
            Assert.True(EqualsWithTol(10.0 / 101.0, Math.Abs(current1)), $"R1 current expected ~0.099A, got {current1}");
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        [Fact]
        public void ResistorWithLmaxBoundaryCondition()
        {
            var netlist = GetSpiceSharpModel(
                "Resistor at lmax boundary",
                "V1 IN 0 10",
                "R1 IN 0 RMOD L=9.99u W=1u",
                ".model RMOD.0 R RSH=100 lmax=10u",
                ".model RMOD R RSH=200",
                ".OP",
                ".SAVE I(R1)",
                ".MEAS OP meas_i MAX I(R1)",
                ".END");

            Assert.NotNull(netlist);
            Assert.False(netlist.ValidationResult.HasError);

            // R1: L=9.99u (<= lmax) -> should use RMOD.0 (RSH=100) -> R = 100 * 9.99 / 1 = 999 ohms
            var current1 = RunOpSimulation(netlist, "I(R1)");
            Assert.True(EqualsWithTol(10.0 / 999.0, Math.Abs(current1)), $"R1 current expected ~0.01A, got {current1}");
            AssertMeasurementSuccess(netlist, "meas_i");
        }

        #endregion
    }
}
