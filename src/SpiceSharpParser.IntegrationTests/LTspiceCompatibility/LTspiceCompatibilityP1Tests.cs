using System;
using System.IO;
using System.Linq;
using System.Text;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.LTspiceCompatibility
{
    public class LTspiceCompatibilityP1Tests : BaseTests
    {
        [Fact]
        public void When_BackannoIsReadInDefaultMode_Expect_TargetedError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "LTspice P1 - default backanno",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".backanno",
                ".op",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, ".backanno");
        }

        [Fact]
        public void When_BackannoIsReadInLtspiceMode_Expect_WarningNoOpAndReference()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P1 - ltspice backanno",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".backanno",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoErrors(model.ValidationResult);
            AssertWarningContains(model.ValidationResult, ".backanno");
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
        }

        [Theory]
        [InlineData("plotwinsize=0", "plotwinsize")]
        [InlineData("plotreltol=0.001", "plotreltol")]
        [InlineData("plotvntol=1u", "plotvntol")]
        [InlineData("plotabstol=1p", "plotabstol")]
        [InlineData("numdgt=6", "numdgt")]
        [InlineData("measdgt=6", "measdgt")]
        [InlineData("meascplxfmt=polar", "meascplxfmt")]
        [InlineData("baudrate=115200", "baudrate")]
        [InlineData("fastaccess", "fastaccess")]
        public void When_LtspiceNoOpOptionIsReadInLtspiceMode_Expect_WarningNoOpAndReference(string optionText, string optionName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P1 - no-op option",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".options " + optionText,
                ".op",
                ".save V(out)",
                ".end");

            AssertNoErrors(model.ValidationResult);
            AssertWarningContains(model.ValidationResult, optionName);
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_PlotwinsizeIsReadInDefaultMode_Expect_UnsupportedOptionError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "LTspice P1 - default plotwinsize",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".options plotwinsize=0",
                ".op",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "plotwinsize");
        }

        [Theory]
        [InlineData("cshunt", "1p")]
        [InlineData("gshunt", "1p")]
        [InlineData("srcsteps", "10")]
        [InlineData("gminsteps", "10")]
        [InlineData("trtol", "7")]
        [InlineData("chgtol", "1p")]
        [InlineData("pivrel", "1e-3")]
        [InlineData("pivtol", "1e-13")]
        [InlineData("ptrantau", "1u")]
        public void When_BehaviorChangingLtspiceOptionIsRead_Expect_TargetedError(string optionName, string optionValue)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P1 - behavior option",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".options " + optionName + "=" + optionValue,
                ".op",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, optionName);
        }

        [Fact]
        public void When_OneArgumentTranIsReadInLtspiceMode_Expect_DerivedStepAndTransientResults()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P1 - one-argument tran",
                "VIN in 0 PULSE(0 1 0 1n 1n 10n 20n)",
                "R1 in out 1k",
                "C1 out 0 1n",
                ".tran 50n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var transient = Assert.IsAssignableFrom<Transient>(model.Simulations.Single());
            var method = Assert.IsAssignableFrom<SpiceMethod>(transient.TimeParameters);
            Assert.True(EqualsWithTol(1e-9, method.InitialStep));
            Assert.True(EqualsWithTol(1e-9, method.MaxStep));
            Assert.True(EqualsWithTol(50e-9, transient.TimeParameters.StopTime));

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
        }

        [Fact]
        public void When_OneArgumentTranWithUicIsReadInLtspiceMode_Expect_UseIcAndTransientResults()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P1 - one-argument tran uic",
                "C1 out 0 1n IC=0",
                "R1 in out 1k",
                "V1 in 0 1",
                ".tran 10n UIC",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var transient = Assert.IsAssignableFrom<Transient>(model.Simulations.Single());
            var method = Assert.IsAssignableFrom<SpiceMethod>(transient.TimeParameters);
            Assert.True(transient.TimeParameters.UseIc);
            Assert.True(EqualsWithTol(0.2e-9, method.InitialStep));
            Assert.True(EqualsWithTol(0.2e-9, method.MaxStep));
            Assert.True(EqualsWithTol(10e-9, transient.TimeParameters.StopTime));

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
        }

        [Fact]
        public void When_OneArgumentTranIsReadInDefaultMode_Expect_ValidationError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "LTspice P1 - default one-argument tran",
                "VIN in 0 PULSE(0 1 0 1n 1n 10n 20n)",
                "R1 in out 1k",
                "C1 out 0 1n",
                ".tran 50n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, ".TRAN");
        }

        [Theory]
        [InlineData("startup")]
        [InlineData("steady")]
        [InlineData("nodiscard")]
        [InlineData("step")]
        public void When_UnsupportedLtspiceTranModifierIsRead_Expect_TargetedError(string modifier)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P1 - tran modifier",
                "VIN in 0 PULSE(0 1 0 1n 1n 10n 20n)",
                "R1 in out 1k",
                "C1 out 0 1n",
                ".tran 50n " + modifier,
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, modifier);
        }

        [Theory]
        [InlineData(".include \"vendor\\models\\load.inc\"")]
        [InlineData(".include vendor/models/load.inc")]
        public void When_LtspiceIncludePathUsesQuotesOrSeparators_Expect_IncludedElement(string includeStatement)
        {
            string tempDirectory = CreateTempDirectory();

            try
            {
                string modelDirectory = Path.Combine(tempDirectory, "vendor", "models");
                Directory.CreateDirectory(modelDirectory);
                File.WriteAllText(Path.Combine(modelDirectory, "load.inc"), "RLOAD out 0 1k" + Environment.NewLine);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P1 - include path fixture",
                    "VIN out 0 1",
                    includeStatement,
                    ".op",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);
                Assert.NotNull(model.Circuit["RLOAD"]);
                Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Fact]
        public void When_LtspiceNestedIncludeUsesIncludedFileDirectory_Expect_IncludedElement()
        {
            string tempDirectory = CreateTempDirectory();

            try
            {
                string vendorDirectory = Path.Combine(tempDirectory, "vendor");
                string partsDirectory = Path.Combine(vendorDirectory, "parts");
                Directory.CreateDirectory(partsDirectory);
                File.WriteAllText(Path.Combine(vendorDirectory, "top.inc"), ".include \"parts\\load.inc\"" + Environment.NewLine);
                File.WriteAllText(Path.Combine(partsDirectory, "load.inc"), "RLOAD out 0 1k" + Environment.NewLine);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P1 - nested include path fixture",
                    "VIN out 0 1",
                    ".include \"vendor/top.inc\"",
                    ".op",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);
                Assert.NotNull(model.Circuit["RLOAD"]);
                Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Fact]
        public void When_LtspiceSelectedLibUsesQuotedWindowsPath_Expect_SelectedContent()
        {
            string tempDirectory = CreateTempDirectory();

            try
            {
                string vendorDirectory = Path.Combine(tempDirectory, "vendor");
                Directory.CreateDirectory(vendorDirectory);
                File.WriteAllText(
                    Path.Combine(vendorDirectory, "loads.lib"),
                    ".lib fast" + Environment.NewLine
                    + "RFAST out 0 10k" + Environment.NewLine
                    + ".endl" + Environment.NewLine
                    + ".lib slow" + Environment.NewLine
                    + "RSLOW out 0 1k" + Environment.NewLine
                    + ".endl" + Environment.NewLine);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P1 - selected lib path fixture",
                    "VIN out 0 1",
                    ".lib \"vendor\\loads.lib\" slow",
                    ".op",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);
                Assert.NotNull(model.Circuit["RSLOW"]);
                Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Fact]
        public void When_LtspiceNestedSelectedLibUsesParentLibDirectory_Expect_SelectedContent()
        {
            string tempDirectory = CreateTempDirectory();

            try
            {
                string vendorDirectory = Path.Combine(tempDirectory, "vendor");
                string innerDirectory = Path.Combine(vendorDirectory, "inner");
                string modelDirectory = Path.Combine(innerDirectory, "models");
                Directory.CreateDirectory(modelDirectory);
                File.WriteAllText(
                    Path.Combine(vendorDirectory, "outer.lib"),
                    ".lib selected" + Environment.NewLine
                    + ".lib \"inner\\loads.lib\" load" + Environment.NewLine
                    + ".endl" + Environment.NewLine);
                File.WriteAllText(
                    Path.Combine(innerDirectory, "loads.lib"),
                    ".lib load" + Environment.NewLine
                    + ".include \"models\\load.inc\"" + Environment.NewLine
                    + ".endl" + Environment.NewLine);
                File.WriteAllText(Path.Combine(modelDirectory, "load.inc"), "RLOAD out 0 1k" + Environment.NewLine);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P1 - nested selected lib path fixture",
                    "VIN out 0 1",
                    ".lib \"vendor\\outer.lib\" selected",
                    ".op",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);
                Assert.NotNull(model.Circuit["RLOAD"]);
                Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
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
            string tempDirectory = Path.Combine(Path.GetTempPath(), "SpiceSharpParserLtspiceP1_" + Guid.NewGuid().ToString("N"));
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

        private static void AssertNoErrors(ValidationEntryCollection validation)
        {
            string messages = string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
            Assert.False(validation.HasError, "Unexpected validation error: " + messages);
        }

        private static void AssertNoValidationIssues(ValidationEntryCollection validation)
        {
            string messages = string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
            Assert.False(validation.HasError, "Unexpected validation error: " + messages);
            Assert.False(validation.HasWarning, "Unexpected validation warning: " + messages);
        }

        private static void AssertWarningContains(ValidationEntryCollection validation, string expected)
        {
            Assert.True(validation.HasWarning, "Expected validation warning containing: " + expected);
            string messages = string.Join(Environment.NewLine, validation.Warnings.Select(warning => warning.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }

        private static void AssertErrorContains(ValidationEntryCollection validation, string expected)
        {
            string messages = string.Join(Environment.NewLine, validation.Errors.Select(error => error.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }
    }
}
