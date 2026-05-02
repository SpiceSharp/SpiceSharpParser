using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.AnalogBehavioralModeling
{
    public class LaplaceTests : BaseTests
    {
        public static IEnumerable<object[]> InvalidLaplaceInputs
        {
            get
            {
                yield return new object[] { "V(a)-V(b)", "input expression" };
                yield return new object[] { "I(Vsense)", "input expression" };
                yield return new object[] { "V(a,b,c)", "input expression" };
                yield return new object[] { "V(a+1)", "input expression" };
            }
        }

        public static IEnumerable<object[]> RejectedLaplaceTransfers
        {
            get
            {
                yield return new object[] { "1/s", "singular DC gain" };
                yield return new object[] { "s", "improper" };
                yield return new object[] { "sin(s)", "rational polynomial" };
            }
        }

        public static IEnumerable<object[]> UnsupportedLaplaceOptions
        {
            get
            {
                yield return new object[] { "M=2", "multiplier" };
                yield return new object[] { "TD=1n", "delay" };
                yield return new object[] { "DELAY=1n", "delay" };
            }
        }

        [Fact]
        public void When_ELaplaceLowPassRunsOp_Expect_DcGainNearOne()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE low-pass OP",
                ".PARAM tau=1u",
                "VIN in 0 1",
                "ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasWarning);
            Assert.False(model.ValidationResult.HasError);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["ELOW"]);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(1.0, export), $"Expected OP gain near 1, got {export}.");
        }

        [Fact]
        public void When_ELaplaceLowPassUsesDifferentialInput_Expect_ControlNodeDifference()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE differential OP",
                ".PARAM tau=1u",
                "VINP inp 0 2",
                "VINN inn 0 0.5",
                "ELOW out 0 LAPLACE {V(inp,inn)} = {1/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasWarning);
            Assert.False(model.ValidationResult.HasError);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(1.5, export), $"Expected OP gain from differential input near 1.5, got {export}.");
        }

        [Fact]
        public void When_ELaplaceLowPassRunsAcAtCutoff_Expect_ExpectedMagnitudeAndPhase()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE low-pass AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "ELOW out 0 LAPLACE {V(in)} = {wc/(s+wc)}",
                "RLOAD out 0 1k",
                ".AC DEC 20 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasWarning);
            Assert.False(model.ValidationResult.HasError);

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vp_fc");
            double magnitude = model.Measurements["vm_fc"][0].Value;
            double phase = model.Measurements["vp_fc"][0].Value;
            Assert.True(Math.Abs(magnitude - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected low-pass cutoff magnitude near 0.707, got {magnitude}.");
            Assert.True(Math.Abs(phase - (-Math.PI / 4.0)) < 0.08, $"Expected low-pass cutoff phase near -pi/4, got {phase}.");
        }

        [Fact]
        public void When_ELaplaceHighPassRunsAcAtCutoff_Expect_ExpectedMagnitudeAndPhase()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE high-pass AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "EHIGH out 0 LAPLACE {V(in)} = {s/(s+wc)}",
                "RLOAD out 0 1k",
                ".AC DEC 20 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasWarning);
            Assert.False(model.ValidationResult.HasError);

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vp_fc");
            double magnitude = model.Measurements["vm_fc"][0].Value;
            double phase = model.Measurements["vp_fc"][0].Value;
            Assert.True(Math.Abs(magnitude - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected high-pass cutoff magnitude near 0.707, got {magnitude}.");
            Assert.True(Math.Abs(phase - (Math.PI / 4.0)) < 0.08, $"Expected high-pass cutoff phase near pi/4, got {phase}.");
        }

        [Fact]
        public void When_ELaplaceIsMixedWithExistingAbmSources_Expect_AllOutputs()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE mixed ABM OP",
                "VIN in 0 2",
                "EVALUE vout 0 VALUE = {V(in)+1}",
                "EPOLY pout 0 POLY(1) in 0 2 1",
                "ETABLE tout 0 TABLE {V(in)} = (0,0) (2,5) (4,9)",
                "ELAPLACE lout 0 LAPLACE {V(in)} = {1/(1+s*1u)}",
                "RV vout 0 1k",
                "RP pout 0 1k",
                "RT tout 0 1k",
                "RL lout 0 1k",
                ".OP",
                ".SAVE V(vout) V(pout) V(tout) V(lout)",
                ".END");

            AssertNoValidationErrors(model);

            double[] exports = RunOpSimulation(model, "V(vout)", "V(pout)", "V(tout)", "V(lout)");
            Assert.True(EqualsWithTol(exports, new[] { 3.0, 4.0, 5.0, 2.0 }));
        }

        [Theory]
        [MemberData(nameof(InvalidLaplaceInputs))]
        public void When_ELaplaceInputIsInvalid_Expect_ReaderValidationError(
            string inputExpression,
            string expectedMessage)
        {
            var model = ReadSingleLaplaceSource(inputExpression, "1/(1+s)");

            AssertReaderErrorContains(model, expectedMessage);
        }

        [Theory]
        [MemberData(nameof(RejectedLaplaceTransfers))]
        public void When_ELaplaceTransferIsRejected_Expect_ReaderValidationError(
            string transferExpression,
            string expectedMessage)
        {
            var model = ReadSingleLaplaceSource("V(in)", transferExpression);

            AssertReaderErrorContains(model, expectedMessage);
        }

        [Theory]
        [MemberData(nameof(UnsupportedLaplaceOptions))]
        public void When_ELaplaceUnsupportedOptionIsUsed_Expect_ReaderValidationError(
            string option,
            string expectedMessage)
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE unsupported option",
                "VIN in 0 1",
                $"EBAD out 0 LAPLACE {{V(in)}} = {{1/(1+s)}} {option}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, expectedMessage);
        }

        [Fact]
        public void When_GLaplaceIsUsed_Expect_ReaderValidationError()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE unsupported",
                "VIN in 0 1",
                "GBAD out 0 LAPLACE {V(in)} = {1/(1+s)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, "G mapping remains unsupported");
        }

        [Fact]
        public void When_HLaplaceIsUsed_Expect_ReaderValidationError()
        {
            var model = GetSpiceSharpModel(
                "H LAPLACE unsupported",
                "VIN in 0 1",
                "HBAD out 0 LAPLACE {V(in)} = {1/(1+s)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, "only for E");
        }

        private static SpiceSharpModel ReadSingleLaplaceSource(string inputExpression, string transferExpression)
        {
            return GetSpiceSharpModel(
                "E LAPLACE validation",
                "VIN in 0 1",
                $"EBAD out 0 LAPLACE {{{inputExpression}}} = {{{transferExpression}}}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");
        }

        private static void AssertNoValidationErrors(SpiceSharpModel model)
        {
            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasWarning);
            Assert.False(model.ValidationResult.HasError);
        }

        private static void AssertReaderErrorContains(SpiceSharpModel model, string expectedMessage)
        {
            Assert.NotNull(model);
            Assert.False(model.ValidationResult.HasWarning);
            Assert.True(model.ValidationResult.HasError);
            var messages = string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message));
            Assert.Contains(expectedMessage, messages, StringComparison.OrdinalIgnoreCase);
        }
    }
}
