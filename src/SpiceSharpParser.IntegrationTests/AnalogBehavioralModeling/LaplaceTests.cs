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

        public static IEnumerable<object[]> InvalidLaplaceOptions
        {
            get
            {
                yield return new object[] { "M=2 M=3", "only once" };
                yield return new object[] { "TD=1n TD=2n", "only once" };
                yield return new object[] { "TD=1n DELAY=2n", "only once" };
                yield return new object[] { "TD=-1n", "non-negative" };
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
        public void When_ELaplaceNoEqualsSyntaxRunsOp_Expect_DcGainNearOne()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE no-equals OP",
                ".PARAM tau=1u",
                "VIN in 0 1",
                "ELOW out 0 LAPLACE {V(in)} {1/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["ELOW"]);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(1.0, export), $"Expected OP gain near 1, got {export}.");
        }

        [Fact]
        public void When_ELaplaceMultiplierRunsOp_Expect_DcGainScaled()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE multiplier OP",
                ".PARAM tau=1u",
                "VIN in 0 1",
                "ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)} M=2",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(2.0, export), $"Expected OP gain near 2, got {export}.");
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
        public void When_GLaplaceLowPassRunsOp_Expect_LoadVoltageWithCurrentSourceSign()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE low-pass OP",
                ".PARAM gm=1m",
                ".PARAM tau=1u",
                "VIN in 0 1",
                "GLOW out 0 LAPLACE {V(in)} = {gm/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledCurrentSource>(model.Circuit["GLOW"]);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(-1.0, export), $"Expected OP load voltage near -1, got {export}.");
        }

        [Fact]
        public void When_GLaplaceLowPassUsesDifferentialInput_Expect_ControlNodeDifference()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE differential OP",
                ".PARAM gm=1m",
                ".PARAM tau=1u",
                "VINP inp 0 2",
                "VINN inn 0 0.5",
                "GLOW out 0 LAPLACE {V(inp,inn)} = {gm/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(-1.5, export), $"Expected OP load voltage from differential input near -1.5, got {export}.");
        }

        [Fact]
        public void When_GLaplaceEqualsAfterKeywordSyntaxRunsOp_Expect_LoadVoltageWithCurrentSourceSign()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE equals-after-keyword OP",
                ".PARAM gm=1m",
                ".PARAM tau=1u",
                "VIN in 0 1",
                "GLOW out 0 LAPLACE = {V(in)} {gm/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledCurrentSource>(model.Circuit["GLOW"]);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(-1.0, export), $"Expected OP load voltage near -1, got {export}.");
        }

        [Fact]
        public void When_GLaplaceMultiplierRunsOp_Expect_LoadVoltageScaled()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE multiplier OP",
                ".PARAM gm=1m",
                ".PARAM tau=1u",
                "VIN in 0 1",
                "GLOW out 0 LAPLACE {V(in)} = {gm/(1+s*tau)} M=2",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(-2.0, export), $"Expected OP load voltage near -2, got {export}.");
        }

        [Fact]
        public void When_GLaplaceLowPassRunsAcAtCutoff_Expect_ExpectedMagnitudeAndPhase()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE low-pass AC",
                ".PARAM gm=1m",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "GLOW out 0 LAPLACE {V(in)} = {gm*wc/(s+wc)}",
                "RLOAD out 0 1k",
                ".AC DEC 20 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vp_fc");
            double magnitude = model.Measurements["vm_fc"][0].Value;
            double phase = model.Measurements["vp_fc"][0].Value;
            Assert.True(Math.Abs(magnitude - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected G low-pass cutoff magnitude near 0.707, got {magnitude}.");
            Assert.True(Math.Abs(phase - (3.0 * Math.PI / 4.0)) < 0.08, $"Expected G low-pass cutoff phase near 3*pi/4, got {phase}.");
        }

        [Fact]
        public void When_GLaplaceHighPassRunsAcAtCutoff_Expect_ExpectedMagnitudeAndPhase()
        {
            var model = GetSpiceSharpModel(
                "G LAPLACE high-pass AC",
                ".PARAM gm=1m",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "GHIGH out 0 LAPLACE {V(in)} = {gm*s/(s+wc)}",
                "RLOAD out 0 1k",
                ".AC DEC 20 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vp_fc");
            double magnitude = model.Measurements["vm_fc"][0].Value;
            double phase = model.Measurements["vp_fc"][0].Value;
            Assert.True(Math.Abs(magnitude - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected G high-pass cutoff magnitude near 0.707, got {magnitude}.");
            Assert.True(Math.Abs(phase - (-3.0 * Math.PI / 4.0)) < 0.08, $"Expected G high-pass cutoff phase near -3*pi/4, got {phase}.");
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

        [Fact]
        public void When_ELaplaceDelayOptionsRunOp_Expect_ValidCircuit()
        {
            var tdModel = GetSpiceSharpModel(
                "E LAPLACE TD OP",
                "VIN in 0 1",
                "ETD out 0 LAPLACE {V(in)} = {1/(1+s*1u)} TD=1n",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            var delayModel = GetSpiceSharpModel(
                "E LAPLACE DELAY OP",
                "VIN in 0 1",
                "EDELAY out 0 LAPLACE {V(in)} = {1/(1+s*1u)} DELAY=1n",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(tdModel);
            AssertNoValidationErrors(delayModel);
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(tdModel, "V(out)")));
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(delayModel, "V(out)")));
        }

        [Fact]
        public void When_ELaplaceLowPassRunsTransient_Expect_FirstOrderStepResponse()
        {
            const double tau = 1e-6;
            var model = GetSpiceSharpModel(
                "E LAPLACE low-pass TRAN",
                ".PARAM tau=1u",
                "VIN in 0 PULSE(0 1 0 1n 1n 100u 200u)",
                "ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".TRAN 50n 8u",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            var exports = RunTransientSimulation(model, "V(out)");
            AssertTransientPoint(exports, 1.0 * tau, FirstOrderStep(1.0), 0.06);
            AssertTransientPoint(exports, 2.0 * tau, FirstOrderStep(2.0), 0.06);
            AssertTransientPoint(exports, 5.0 * tau, FirstOrderStep(5.0), 0.04);
        }

        [Fact]
        public void When_GLaplaceLowPassRunsTransient_Expect_FirstOrderStepResponseWithCurrentSourceSign()
        {
            const double tau = 1e-6;
            var model = GetSpiceSharpModel(
                "G LAPLACE low-pass TRAN",
                ".PARAM tau=1u",
                ".PARAM gm=1m",
                "VIN in 0 PULSE(0 1 0 1n 1n 100u 200u)",
                "GLOW out 0 LAPLACE {V(in)} = {gm/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".TRAN 50n 8u",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            var exports = RunTransientSimulation(model, "V(out)");
            AssertTransientPoint(exports, 1.0 * tau, -FirstOrderStep(1.0), 0.06);
            AssertTransientPoint(exports, 2.0 * tau, -FirstOrderStep(2.0), 0.06);
            AssertTransientPoint(exports, 5.0 * tau, -FirstOrderStep(5.0), 0.04);
        }

        [Fact]
        public void When_ELaplaceDelayedLowPassRunsTransient_Expect_NoRuntimeException()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE delayed low-pass TRAN",
                ".PARAM tau=1u",
                "VIN in 0 PULSE(0 1 0 1n 1n 100u 200u)",
                "ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)} TD=2u",
                "RLOAD out 0 1k",
                ".TRAN 50n 8u",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            var exception = Record.Exception(() => RunTransientSimulation(model, "V(out)"));
            Assert.Null(exception);
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
        [MemberData(nameof(InvalidLaplaceOptions))]
        public void When_ELaplaceInvalidOptionIsUsed_Expect_ReaderValidationError(
            string option,
            string expectedMessage)
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE invalid option",
                "VIN in 0 1",
                $"EBAD out 0 LAPLACE {{V(in)}} = {{1/(1+s)}} {option}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, expectedMessage);
        }

        [Fact]
        public void When_ValueLaplaceFunctionIsUsed_Expect_ReaderValidationError()
        {
            var model = GetSpiceSharpModel(
                "E VALUE LAPLACE unsupported",
                "VIN in 0 1",
                "EBAD out 0 VALUE = {LAPLACE(V(in), 1/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, "function syntax");
        }

        [Fact]
        public void When_BVoltageLaplaceFunctionIsUsed_Expect_ReaderValidationError()
        {
            var model = GetSpiceSharpModel(
                "B LAPLACE unsupported",
                "VIN in 0 1",
                "BBAD out 0 V={LAPLACE(V(in), 1/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, "function syntax");
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

            AssertReaderErrorContains(model, "E and G");
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

        private static void AssertTransientPoint(
            IReadOnlyList<Tuple<double, double>> exports,
            double time,
            double expected,
            double tolerance)
        {
            var nearest = exports.OrderBy(export => Math.Abs(export.Item1 - time)).First();
            Assert.True(
                Math.Abs(nearest.Item2 - expected) <= tolerance,
                $"Expected V(out) near {expected} at {time}, got {nearest.Item2} at {nearest.Item1}.");
        }

        private static double FirstOrderStep(double normalizedTime)
        {
            return 1.0 - Math.Exp(-normalizedTime);
        }
    }
}
