using System;
using System.Linq;
using System.Text;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.LTspiceCompatibility
{
    public class LTspiceCompatibilityP3Tests : BaseTests
    {
        [Fact]
        public void When_RlcModelTcAliasIsRead_Expect_Tc1AndTc2AreMapped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - R/C tc aliases",
                ".model rmod R(RSH=1 tc=0.01,0.02)",
                ".model cmod C(CJ=1n tc=0.03,0.04)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var resistorModel = Assert.IsType<ResistorModel>(model.Circuit["rmod"]);
            Assert.Equal(0.01, resistorModel.Parameters.TemperatureCoefficient1);
            Assert.Equal(0.02, resistorModel.Parameters.TemperatureCoefficient2);

            var capacitorModel = Assert.IsType<CapacitorModel>(model.Circuit["cmod"]);
            Assert.Equal(0.03, capacitorModel.Parameters.TemperatureCoefficient1);
            Assert.Equal(0.04, capacitorModel.Parameters.TemperatureCoefficient2);
        }

        [Fact]
        public void When_SwitchThresholdAliasesAreRead_Expect_MidpointAndHysteresisAreMapped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - switch aliases",
                ".model vsw SW(Ron=10 Roff=1Meg von=2 voff=0)",
                ".model isw CSW(Ron=10 Roff=1Meg ion=2m ioff=0)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var voltageSwitch = Assert.IsType<VoltageSwitchModel>(model.Circuit["vsw"]);
            Assert.Equal(1.0, voltageSwitch.Parameters.Threshold);
            Assert.Equal(1.0, voltageSwitch.Parameters.Hysteresis);

            var currentSwitch = Assert.IsType<CurrentSwitchModel>(model.Circuit["isw"]);
            Assert.Equal(1e-3, currentSwitch.Parameters.Threshold);
            Assert.Equal(1e-3, currentSwitch.Parameters.Hysteresis);
        }

        [Fact]
        public void When_LtspiceMetadataParametersAreRead_Expect_WarningsOnly()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - metadata no-ops",
                "R1 out 0 1k mfg=Acme pn=R123",
                ".model rmod R(RSH=1 mfg=Acme desc=thinfilm)",
                ".end");

            Assert.False(model.ValidationResult.HasError, ValidationMessages(model.ValidationResult));
            Assert.True(model.ValidationResult.HasWarning);
            AssertWarningContains(model.ValidationResult, "mfg");
            AssertWarningContains(model.ValidationResult, "pn");
            AssertWarningContains(model.ValidationResult, "desc");
        }

        [Fact]
        public void When_MetadataParametersAreReadWithoutLtspiceCompatibility_Expect_DefaultErrors()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "LTspice P3 - default metadata",
                ".model rmod R(RSH=1 mfg=Acme)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "mfg");
        }

        [Theory]
        [InlineData("R1 out 0 1k Rser=1", "Rser", "parasitic")]
        [InlineData("C1 out 0 1n Lser=1n", "Lser", "parasitic")]
        [InlineData("L1 out 0 1u Cpar=1p", "Cpar", "parasitic")]
        [InlineData("C1 out 0 Q=1n*x", "Q", "charge-defined")]
        [InlineData("L1 out 0 Flux=1m*tanh(x)", "Flux", "flux-defined")]
        public void When_LtspicePassiveInstanceParameterChangesTopology_Expect_TargetedError(
            string componentLine,
            string parameterName,
            string expectedReason)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - passive instance parameter",
                componentLine,
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, parameterName);
            AssertErrorContains(model.ValidationResult, expectedReason);
        }

        [Theory]
        [InlineData("Ron")]
        [InlineData("Roff")]
        [InlineData("Vfwd")]
        [InlineData("Ilimit")]
        [InlineData("Epsilon")]
        public void When_LtspiceIdealDiodeModelParameterIsRead_Expect_TargetedError(string parameterName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - ideal diode",
                $".model dmod D({parameterName}=1)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, parameterName);
            AssertErrorContains(model.ValidationResult, "ideal-diode");
        }

        [Theory]
        [InlineData("Lser")]
        [InlineData("Vser")]
        [InlineData("Ilimit")]
        public void When_LtspiceSwitchModelParameterChangesBehavior_Expect_TargetedError(string parameterName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - switch unsupported parameter",
                $".model smod SW(Ron=1 Roff=1Meg Vt=0 Vh=0 {parameterName}=1)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, parameterName);
            AssertErrorContains(model.ValidationResult, "switch");
        }

        [Fact]
        public void When_LtspiceVdmosModelIsRead_Expect_EngineRequiredError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - VDMOS",
                ".model pwr VDMOS(Ron=1 Vto=2)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "VDMOS");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        [Fact]
        public void When_LtspiceHighMosLevelIsRead_Expect_TargetedError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - MOS level",
                ".model nmod NMOS(level=8)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "LTspice");
            AssertErrorContains(model.ValidationResult, "level 8");
        }

        [Fact]
        public void When_LtspiceThreeTerminalMosSyntaxIsRead_Expect_TargetedError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - three-terminal MOS",
                "M1 d g s pwr",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "three-terminal MOS");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        [Fact]
        public void When_LtspiceLossyTransmissionLineIsRead_Expect_EngineRequiredError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - LTRA",
                "O1 in 0 out 0 lossy",
                ".model lossy LTRA(R=1 L=1u C=1n len=1)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "O/LTRA");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        [Fact]
        public void When_LtspiceUniformRcLineIsRead_Expect_EngineRequiredError()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P3 - URC",
                "U1 a b 0 urc L=1",
                ".model urc URC(Rperl=1 Cperl=1p)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "U/URC");
            AssertErrorContains(model.ValidationResult, "engine support");
        }

        private static SpiceSharpModel GetSpiceSharpModelWithCompatibility(CompatibilityOptions compatibility, params string[] lines)
        {
            var text = string.Join(Environment.NewLine, lines);
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
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

        private static void AssertNoValidationIssues(ValidationEntryCollection validation)
        {
            var messages = ValidationMessages(validation);
            Assert.False(validation.HasError, "Unexpected validation error: " + messages);
            Assert.False(validation.HasWarning, "Unexpected validation warning: " + messages);
        }

        private static void AssertErrorContains(ValidationEntryCollection validation, string expected)
        {
            var messages = string.Join(Environment.NewLine, validation.Errors.Select(error => error.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }

        private static void AssertWarningContains(ValidationEntryCollection validation, string expected)
        {
            var messages = string.Join(Environment.NewLine, validation.Warnings.Select(warning => warning.Message));
            Assert.Contains(expected, messages, StringComparison.OrdinalIgnoreCase);
        }

        private static string ValidationMessages(ValidationEntryCollection validation)
        {
            return string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
        }
    }
}
