using System;
using System.IO;
using System.Linq;
using System.Text;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.LTspiceCompatibility
{
    public class LTspiceCompatibilityP2Tests : BaseTests
    {
        [Fact]
        public void When_LtspiceScalarFunctionsAreRead_Expect_ReferenceValue()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - scalar functions",
                "V1 out 0 VALUE={arccos(1)+arcsin(1)+arctan(1)+fabs(-3)+sgn(-2)+round(1.6)+pwr(2,3)+pwrs(-2,3)+hypot(3,4)+table(1.5,0,0,1,10,2,20)+tbl(1.5,0,0,1,10,2,20)+2**3}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(47.0 + (0.75 * Math.PI), RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_ExpWaveformIsRead_Expect_TransientReferenceValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - EXP waveform",
                "V1 out 0 EXP(0 1 1n 1n 4n 1n)",
                "R1 out 0 1k",
                ".tran 1n 6n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, ExpReference));
        }

        [Fact]
        public void When_LtspiceTblSourceOptionIsRead_Expect_TableBehavioralSource()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - source tbl",
                "V1 out 0 tbl=(0.5,0,0,1,10)",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(5.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_LtspicePwlFileIsRead_Expect_TransientReferenceValues()
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                File.WriteAllText(
                    Path.Combine(tempDirectory, "shape.txt"),
                    "time,value" + Environment.NewLine
                    + "0,0" + Environment.NewLine
                    + "1e-9,1" + Environment.NewLine
                    + "2e-9,0" + Environment.NewLine);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - PWL file",
                    "V1 out 0 PWL file=\"shape.txt\"",
                    "R1 out 0 1k",
                    ".tran 1n 2n",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);

                var exports = RunTransientSimulation(model, "V(out)");
                Assert.NotEmpty(exports);
                Assert.True(EqualsWithTol(exports, PwlFileReference));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Theory]
        [InlineData("V1 out 0 PWL file=\"missing.txt\"", "does not exist")]
        [InlineData("V1 out 0 PWL repeat 0 0 1n 1 endrepeat", "repeat")]
        public void When_LtspicePwlFileOrRepeatSyntaxIsUnsupported_Expect_TargetedError(string sourceLine, string expectedMessage)
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - invalid PWL",
                    sourceLine,
                    "R1 out 0 1k",
                    ".tran 1n 2n",
                    ".save V(out)",
                    ".end");

                Assert.True(model.ValidationResult.HasError);
                AssertErrorContains(model.ValidationResult, "PWL");
                AssertErrorContains(model.ValidationResult, expectedMessage);
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Theory]
        [InlineData("", "empty")]
        [InlineData("time,value", "no data")]
        [InlineData("time,value\r\nnot-a-time,1", "malformed")]
        public void When_LtspicePwlFileIsMalformed_Expect_TargetedError(string fileContent, string expectedMessage)
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                File.WriteAllText(Path.Combine(tempDirectory, "bad.txt"), fileContent);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - malformed PWL file",
                    "V1 out 0 PWL file=\"bad.txt\"",
                    "R1 out 0 1k",
                    ".tran 1n 2n",
                    ".save V(out)",
                    ".end");

                Assert.True(model.ValidationResult.HasError);
                AssertErrorContains(model.ValidationResult, "PWL");
                AssertErrorContains(model.ValidationResult, expectedMessage);
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Theory]
        [InlineData("V1 out 0 1 Rser=10", "Rser")]
        [InlineData("V1 out 0 1 Cpar=1p", "Cpar")]
        [InlineData("I1 out 0 1 load=1", "load")]
        [InlineData("V1 out 0 1 R=10", "R")]
        public void When_LtspiceSourceOptionChangesTopology_Expect_TargetedError(string sourceLine, string optionName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - source option",
                sourceLine,
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, optionName);
        }

        [Theory]
        [InlineData("V1 out 0 WAVE chan=1", "wavefile")]
        [InlineData("V1 out 0 wavefile=\"missing.wav\"", "chan")]
        [InlineData("V1 out 0 wavefile=\"missing.wav\" chan=1", "does not exist")]
        public void When_LtspiceWaveFileSourceIsInvalid_Expect_TargetedError(string sourceLine, string expectedMessage)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - invalid wavefile",
                sourceLine,
                "R1 out 0 1k",
                ".tran 1n 6n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "wavefile");
            AssertErrorContains(model.ValidationResult, expectedMessage);
        }

        [Fact]
        public void When_LtspiceWaveFileChannelIsInvalid_Expect_TargetedError()
        {
            var waveFilePath = Path.GetTempFileName();

            try
            {
                var model = GetSpiceSharpModelWithCompatibility(
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - invalid wavefile channel",
                    $"V1 out 0 wavefile=\"{waveFilePath}\" chan=bad",
                    "R1 out 0 1k",
                    ".tran 1n 6n",
                    ".save V(out)",
                    ".end");

                Assert.True(model.ValidationResult.HasError);
                AssertErrorContains(model.ValidationResult, "wavefile");
                AssertErrorContains(model.ValidationResult, "chan");
            }
            finally
            {
                File.Delete(waveFilePath);
            }
        }

        [Theory]
        [InlineData("V1 out 0 SINE(0 1 1k 0 0 0 3)", "SINE")]
        public void When_LtspiceWaveformHasCycleCount_Expect_TargetedError(string sourceLine, string waveformName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - cycle count",
                sourceLine,
                "R1 out 0 1k",
                ".tran 1n 50n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, waveformName);
            AssertErrorContains(model.ValidationResult, "cycle-count");
        }

        [Fact]
        public void When_LtspicePulseHasCycleCount_Expect_TransientStopsAfterFiniteCycles()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - finite-cycle PULSE",
                "V1 out 0 PULSE(0 1 2n 1n 1n 3n 10n 2)",
                "R1 out 0 1k",
                ".tran 1n 25n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, FinitePulseReference));
        }

        [Theory]
        [InlineData("V1 out 0 PULSE(0 1 0 1n 1n 10n 0 3)", "period")]
        [InlineData("V1 out 0 PULSE(0 1 0 1n 1n 10n 20n 0)", "cycle-count")]
        public void When_LtspicePulseCycleCountArgumentsAreInvalid_Expect_TargetedError(
            string sourceLine,
            string expectedMessage)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - invalid finite-cycle PULSE",
                sourceLine,
                "R1 out 0 1k",
                ".tran 1n 50n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "PULSE");
            AssertErrorContains(model.ValidationResult, expectedMessage);
        }

        [Theory]
        [InlineData("uplim(V(in),0,1)", "uplim")]
        [InlineData("dnlim(V(in),0,1)", "dnlim")]
        [InlineData("~V(in)", "~")]
        public void When_LtspiceUnsupportedExpressionConstructIsRead_Expect_TargetedError(string expression, string constructName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - expression diagnostic",
                "VIN in 0 0",
                "B1 out 0 V={" + expression + "}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, constructName);
        }

        [Theory]
        [InlineData("V1 out 0 EXP(0 1 1n 0 4n 1n)", "tau1")]
        [InlineData("V1 out 0 EXP(0 1 1n 1n 4n 0)", "tau2")]
        [InlineData("V1 out 0 EXP(0 1)", "six arguments")]
        public void When_ExpWaveformIsInvalid_Expect_TargetedError(string sourceLine, string expectedMessage)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - invalid EXP",
                sourceLine,
                "R1 out 0 1k",
                ".tran 1n 6n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "EXP");
            AssertErrorContains(model.ValidationResult, expectedMessage);
        }

        private static double ExpReference(double time)
        {
            if (time <= 1e-9)
            {
                return 0.0;
            }

            var value = 1.0 - Math.Exp(-(time - 1e-9) / 1e-9);
            if (time > 4e-9)
            {
                value -= 1.0 - Math.Exp(-(time - 4e-9) / 1e-9);
            }

            return value;
        }

        private static double FinitePulseReference(double time)
        {
            const double initialValue = 0.0;
            const double pulsedValue = 1.0;
            const double delay = 2e-9;
            const double riseTime = 1e-9;
            const double fallTime = 1e-9;
            const double pulseWidth = 3e-9;
            const double period = 10e-9;
            const double cycleCount = 2.0;

            if (time < delay || time >= delay + (period * cycleCount))
            {
                return initialValue;
            }

            var localTime = (time - delay) % period;
            if (localTime < riseTime)
            {
                return initialValue + ((pulsedValue - initialValue) * localTime / riseTime);
            }

            if (localTime < riseTime + pulseWidth)
            {
                return pulsedValue;
            }

            if (localTime < riseTime + pulseWidth + fallTime)
            {
                return pulsedValue + ((initialValue - pulsedValue) * (localTime - riseTime - pulseWidth) / fallTime);
            }

            return initialValue;
        }

        private static double PwlFileReference(double time)
        {
            if (time <= 1e-9)
            {
                return time / 1e-9;
            }

            return Math.Max(0.0, (2e-9 - time) / 1e-9);
        }

        private static SpiceSharpModel GetSpiceSharpModelWithCompatibility(CompatibilityOptions compatibility, params string[] lines)
        {
            return GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(null, compatibility, lines);
        }

        private static SpiceSharpModel GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(string workingDirectory, CompatibilityOptions compatibility, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            parser.Settings.WorkingDirectory = workingDirectory;
            parser.Settings.Compatibility = compatibility;

            var parserResult = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(
                new SpiceNetlistCaseSensitivitySettings(),
                () => parser.Settings.WorkingDirectory,
                Encoding.Default)
            {
                Compatibility = compatibility,
            };

            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);
            return spiceSharpReader.Read(parserResult.FinalModel);
        }

        private static string CreateTempDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), "SpiceSharpParserLtspiceP2_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        private static void DeleteDirectory(string tempDirectory)
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private static void AssertNoValidationIssues(ValidationEntryCollection validation)
        {
            var messages = string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
            Assert.False(validation.HasError, "Unexpected validation error: " + messages);
            Assert.False(validation.HasWarning, "Unexpected validation warning: " + messages);
        }

        private static void AssertErrorContains(ValidationEntryCollection validation, string expected)
        {
            var messages = string.Join(Environment.NewLine, validation.Errors.Select(error => error.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }
    }
}
