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

        private static SpiceNetlistTestOptions ADeviceOptions()
        {
            return new SpiceNetlistTestOptions
            {
                Compatibility = CompatibilityOptions.LTspice,
                UseCustomComponents = true,
            };
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
