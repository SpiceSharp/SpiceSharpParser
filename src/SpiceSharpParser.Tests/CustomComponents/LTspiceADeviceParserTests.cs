using System;
using System.Linq;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Testing;
using Xunit;

namespace SpiceSharpParser.Tests.CustomComponents
{
    public class LTspiceADeviceParserTests
    {
        [Fact]
        public void UseCustomComponents_ReadsEverySupportedADeviceModel()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "Supported LTspice A-devices",
                "ASR set reset 0 0 0 srqb srq 0 SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "ADFF data 0 clock preset clear dqb dq 0 DFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                "APD pa pb 0 0 0 0 pdout 0 PHASEDET Iout=1m",
                "ACOUNT cclk creset 0 0 0 cqb cq 0 COUNTER cycles=4 duty=0.5 Vhigh=5 Vlow=0",
                "ASH input 0 shclk 0 0 0 shout 0 SAMPLEHOLD Rout=1k",
                "AOTA in1n in1p in2p in2n 0 rail otaout 0 OTA G=1m Linear",
                "AVAR control 0 0 0 0 0 varout 0 VARISTOR Rclamp=10",
                "AMOD fm am 0 0 0 0 modout 0 MODULATE mark=2k space=1k",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            Assert.True(model.Circuit.Count > 8);
            Assert.Contains(model.Circuit, entity => entity.Name.StartsWith("XASR", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.Circuit, entity => entity.Name.StartsWith("XAOTA", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.Circuit, entity => entity.Name.StartsWith("XAMOD", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UseCustomComponents_AcceptsModulatorAlias()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "MODULATOR alias",
                "AMOD fm am 0 0 0 0 out 0 MODULATOR mark=2k space=1k",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            Assert.Contains(model.Circuit, entity => entity.Name.StartsWith("XAMOD", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UseCustomComponents_ReportsUnsupportedModel()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "Unsupported A-device",
                "ABAD 1 2 3 4 5 6 7 0 MYSTERY",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("Unsupported LTspice A-device model 'MYSTERY'"));
        }

        [Fact]
        public void UseCustomComponents_ReportsMissingTerminals()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "Malformed A-device",
                "ABAD 1 2 3 4 OTA",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("expects eight terminals followed by a model name"));
        }

        [Fact]
        public void DefaultReader_RequiresCustomComponentOptIn()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                "A-device opt-in",
                "AOTA 1 2 3 4 0 6 7 0 OTA Linear",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("Unsupported component AOTA"));
        }

        [Fact]
        public void UseCustomComponents_RejectsSampleHoldDelayUntilTimingIsImplemented()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "SAMPLEHOLD timing validation",
                "ASH in 0 clk 0 0 0 out 0 SAMPLEHOLD Td=1n",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("'ASH' parameter 'Td'")
                    && error.Message.Contains("not supported"));
            Assert.DoesNotContain(
                model.Circuit,
                entity => entity.Name.StartsWith("XASH", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UseCustomComponents_RejectsUnknownBareFlags()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "Strict A-device flags",
                "AMOD fm am 0 0 0 0 out 0 MODULATE mark",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("Unknown bare flag 'mark'")
                    && error.Message.Contains("'AMOD'"));
        }

        [Theory]
        [InlineData("AMOD fm am 0 0 0 0 out 0 MODULATOR space=1k", "Mark")]
        [InlineData("AMOD fm am 0 0 0 0 out 0 MODULATOR mark=1k", "Space")]
        public void UseCustomComponents_RequiresModulatorFrequencies(
            string device,
            string parameter)
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "Required MODULATOR frequencies",
                device,
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("'AMOD'")
                    && error.Message.Contains($"requires the {parameter} parameter"));
            Assert.DoesNotContain(
                model.Circuit,
                entity => entity.Name.StartsWith("X__a_", StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData("AOTA 0 1 1 0 0 0 out 0 OTA Iout=0", "iout")]
        [InlineData("AOTA 0 1 1 0 0 0 out 0 OTA Isink=10u", "isink")]
        [InlineData("AVAR 1 0 0 0 0 0 out 0 VARISTOR Rclamp=0", "rclamp")]
        [InlineData("ACOUNT clk reset 0 0 0 qb q 0 COUNTER Cycles=2.5", "cycles")]
        [InlineData("ACOUNT clk reset 0 0 0 qb q 0 COUNTER Cycles=4 Duty=1.2", "duty")]
        public void UseCustomComponents_RejectsUnsafeModelParameters(
            string device,
            string parameter)
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "A-device range validation",
                device,
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains($"parameter '{parameter}'"));
            Assert.DoesNotContain(
                model.Circuit,
                entity => entity.Name.StartsWith("X__a_", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UseCustomComponents_RejectsSweepDependentParametersInsteadOfFreezingThem()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "A-device stepped parameter",
                ".param f=1k",
                "AMOD fm am 0 0 0 0 out 0 MODULATE mark={f} space=1k",
                ".step param f list 1k 2k",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            Assert.Contains(
                model.ValidationResult.Errors,
                error => error.Message.Contains("'AMOD' parameter 'mark'")
                    && error.Message.Contains("stepped parameter 'f'"));
        }

        [Fact]
        public void UseCustomComponents_UsesCollisionSafeExpansionName()
        {
            SpiceNetlistTestOptions options = ADeviceOptions();
            options.ExpandSubcircuits = false;
            var model = SpiceNetlistTestHelper.ParseAndRead(
                options,
                "A-device expansion collision",
                ".subckt passthrough p n",
                "R1 p n 1k",
                ".ends passthrough",
                "A1 0 0 0 0 0 0 out 0 OTA Linear",
                "XA1 0 0 passthrough",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            Assert.Contains(model.Circuit, entity => entity.Name.Equals("XA1", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                model.Circuit,
                entity => entity.Name.StartsWith("X__a_A1", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UseCustomComponents_UsesCollisionSafeHierarchicalPrefix()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "A-device hierarchical expansion collision",
                ".subckt shadow p n",
                "RIN1N p n 1k",
                ".ends shadow",
                "A1 0 1 1 0 0 0 out 0 OTA Linear",
                "xa1 0 0 shadow",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            Assert.Contains(
                model.Circuit,
                entity => entity.Name.Equals("xa1.RIN1N", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                model.Circuit,
                entity => entity.Name.StartsWith("X__a_A1.", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void UseCustomComponents_UnusedModulatorAmplitudeDefaultsToOne()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "A-device unused AM terminal",
                "VFM fm 0 0.5",
                "VZERO zero 0 0",
                "ADEFAULT fm 0 0 0 0 0 out_default 0 MODULATE mark=2k space=1k Rout=1",
                "ADRIVEN fm zero 0 0 0 0 out_zero 0 MODULATE mark=2k space=1k Rout=1",
                "RDEFAULT out_default 0 100k",
                "RZERO out_zero 0 100k",
                ".tran 1u 200u 0 1u UIC",
                ".meas tran unused_am FIND V(out_default) AT=166.6666667u",
                ".meas tran driven_zero FIND V(out_zero) AT=166.6666667u",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            SpiceSimulationTestHelper.RunSimulations(model);
            Assert.InRange(MeasurementValue(model, "unused_am"), 0.95, 1.05);
            Assert.InRange(MeasurementValue(model, "driven_zero"), -0.01, 0.01);
        }

        [Fact]
        public void UseCustomComponents_UnusedAndGroundedInputsRemainDistinct()
        {
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "A-device unused and grounded inputs with non-ground common",
                "VCOMMON common 0 1",
                "VINP inp common 2",
                "VTRACK track common 1",
                "AUNUSED inp common common track common common out_unused common SAMPLEHOLD Rout=1",
                "AGROUNDED inp 0 common track common common out_grounded common SAMPLEHOLD Rout=1",
                "RUNUSED out_unused common 100k",
                "RGROUNDED out_grounded common 100k",
                ".tran 10n 1u 0 10n UIC",
                ".meas tran unused FIND V(out_unused,common) AT=500n",
                ".meas tran grounded FIND V(out_grounded,common) AT=500n",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            SpiceSimulationTestHelper.RunSimulations(model);
            Assert.InRange(MeasurementValue(model, "unused"), 1.99, 2.01);
            Assert.InRange(MeasurementValue(model, "grounded"), 2.99, 3.01);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UseCustomComponents_UnusedLatchOutputsAreElectricallyDetached(bool omitQ)
        {
            string setValue = omitQ ? "5" : "0";
            string resetValue = omitQ ? "0" : "5";
            string invertedOutput = omitQ ? "qb" : "common";
            string output = omitQ ? "common" : "q";
            string loadedOutput = omitQ ? "qb" : "q";
            var model = SpiceNetlistTestHelper.ParseAndRead(
                ADeviceOptions(),
                "A-device unused output",
                "VCOMMON common 0 1",
                $"VSET set common {setValue}",
                $"VRESET reset common {resetValue}",
                $"ASR set reset common common common {invertedOutput} {output} common SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1",
                $"RLOAD {loadedOutput} common 10k",
                ".tran 1n 20n 0 1n UIC",
                $".meas tran loaded FIND V({loadedOutput},common) AT=10n",
                ".meas tran common_current FIND I(VCOMMON) AT=10n",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model));
            SpiceSimulationTestHelper.RunSimulations(model);
            Assert.InRange(MeasurementValue(model, "loaded"), -0.01, 0.01);
            Assert.InRange(Math.Abs(MeasurementValue(model, "common_current")), 0.0, 1e-6);
        }

        private static SpiceNetlistTestOptions ADeviceOptions()
        {
            return new SpiceNetlistTestOptions
            {
                Compatibility = CompatibilityOptions.LTspice,
                UseCustomComponents = true,
            };
        }

        private static double MeasurementValue(SpiceSharpModel model, string measurementName)
        {
            Assert.True(
                model.Measurements.TryGetValue(measurementName, out var measurements),
                $"Measurement '{measurementName}' was not produced.");
            var measurement = measurements.Last();
            Assert.True(measurement.Success, $"Measurement '{measurementName}' failed.");
            return measurement.Value;
        }

        private static string ValidationMessages(
            SpiceSharpModel model)
        {
            return string.Join(
                Environment.NewLine,
                model.ValidationResult.Errors.Select(error => error.Message));
        }
    }
}
