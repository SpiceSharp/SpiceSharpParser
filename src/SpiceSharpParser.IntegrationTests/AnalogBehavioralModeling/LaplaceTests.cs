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
        public void When_FLaplaceLowPassRunsOp_Expect_CurrentControlledGain()
        {
            var model = GetSpiceSharpModel(
                "F LAPLACE low-pass OP",
                "V1 1 0 100",
                "R1 1 0 10",
                "FLOW 2 0 LAPLACE {I(V1)} = {1.5/(1+s*1u)}",
                "R2 2 0 2",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceCurrentControlledCurrentSource>(model.Circuit["FLOW"]);

            double export = RunOpSimulation(model, "I(R2)");
            Assert.True(EqualsWithTol(15.0, export), $"Expected OP current near 15, got {export}.");
        }

        [Fact]
        public void When_HLaplaceLowPassRunsOp_Expect_CurrentControlledGain()
        {
            var model = GetSpiceSharpModel(
                "H LAPLACE low-pass OP",
                "V1 1 0 100",
                "R1 1 0 10",
                "HLOW 2 0 LAPLACE {I(V1)} = {1.5/(1+s*1u)}",
                "R2 2 0 2",
                ".SAVE I(R2)",
                ".OP",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceCurrentControlledVoltageSource>(model.Circuit["HLOW"]);

            double export = RunOpSimulation(model, "I(R2)");
            Assert.True(EqualsWithTol(-7.5, export), $"Expected OP current near -7.5, got {export}.");
        }

        [Fact]
        public void When_FLaplaceLowPassRunsAcAtCutoff_Expect_ExpectedMagnitude()
        {
            var model = GetSpiceSharpModel(
                "F LAPLACE low-pass AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "V1 in 0 AC 1",
                "R1 in 0 1",
                "FLOW out 0 LAPLACE {I(V1)} = {wc/(s+wc)}",
                "RLOAD out 0 1",
                ".AC DEC 20 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            double magnitude = model.Measurements["vm_fc"][0].Value;
            Assert.True(Math.Abs(magnitude - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected F low-pass cutoff magnitude near 0.707, got {magnitude}.");
        }

        [Fact]
        public void When_HLaplaceLowPassRunsAcAtCutoff_Expect_ExpectedMagnitude()
        {
            var model = GetSpiceSharpModel(
                "H LAPLACE low-pass AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "V1 in 0 AC 1",
                "R1 in 0 1",
                "HLOW out 0 LAPLACE {I(V1)} = {wc/(s+wc)}",
                "RLOAD out 0 1",
                ".AC DEC 20 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);

            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            double magnitude = model.Measurements["vm_fc"][0].Value;
            Assert.True(Math.Abs(magnitude - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected H low-pass cutoff magnitude near 0.707, got {magnitude}.");
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
        public void When_ValueLaplaceFunctionIsUsed_Expect_DcGainNearOne()
        {
            var model = GetSpiceSharpModel(
                "E VALUE LAPLACE OP",
                "VIN in 0 1",
                "EVALUE out 0 VALUE = {LAPLACE(V(in), 1/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["EVALUE"]);
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_BVoltageLaplaceFunctionIsUsed_Expect_DcGainNearOne()
        {
            var model = GetSpiceSharpModel(
                "B V LAPLACE OP",
                "VIN in 0 1",
                "BLOW out 0 V={LAPLACE(V(in), 1/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["BLOW"]);
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_BCurrentLaplaceFunctionIsUsed_Expect_LoadVoltageWithCurrentSourceSign()
        {
            var model = GetSpiceSharpModel(
                "B I LAPLACE OP",
                ".PARAM gm=1m",
                "VIN in 0 1",
                "BLOW out 0 I={LAPLACE(V(in), gm/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledCurrentSource>(model.Circuit["BLOW"]);
            Assert.True(EqualsWithTol(-1.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_MixedBVoltageLaplaceFunctionIsUsed_Expect_HelperAndOffset()
        {
            var model = GetSpiceSharpModel(
                "B mixed LAPLACE OP",
                "VIN in 0 1",
                "BMIX out 0 V={1 + 2*LAPLACE(V(in), 1/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<BehavioralVoltageSource>(model.Circuit["BMIX"]);
            Assert.Contains(model.Circuit, entity => entity.Name == "__ssp_laplace_BMIX_0_src");
            Assert.True(EqualsWithTol(3.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_MultipleLaplaceFunctionsUseDelay_Expect_ReaderValidationError()
        {
            var model = GetSpiceSharpModel(
                "B mixed LAPLACE invalid delay",
                "VA a 0 1",
                "VB b 0 1",
                "BMIX out 0 V={LAPLACE(V(a), 1/(1+s)) + LAPLACE(V(b), 1/(1+s))} TD=1n",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, "only when one LAPLACE call");
        }

        [Fact]
        public void When_BVoltageLaplaceFunctionUsesInlineMultiplier_Expect_DcGainScaled()
        {
            var model = GetSpiceSharpModel(
                "B V LAPLACE inline multiplier OP",
                "VIN in 0 1",
                "BLOW out 0 V={LAPLACE(V(in), 1/(1+s), M=2)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["BLOW"]);
            Assert.True(EqualsWithTol(2.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_BCurrentLaplaceFunctionUsesInlineMultiplier_Expect_LoadVoltageScaled()
        {
            var model = GetSpiceSharpModel(
                "B I LAPLACE inline multiplier OP",
                ".PARAM gm=1m",
                "VIN in 0 1",
                "BLOW out 0 I={LAPLACE(V(in), gm/(1+s), M=2)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledCurrentSource>(model.Circuit["BLOW"]);
            Assert.True(EqualsWithTol(-2.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_MixedLaplaceFunctionsUseInlineMultipliers_Expect_SummedGain()
        {
            var model = GetSpiceSharpModel(
                "B mixed LAPLACE inline multipliers OP",
                "VA a 0 1",
                "VB b 0 1",
                "BMIX out 0 V={LAPLACE(V(a), 1/(1+s), M=2) + LAPLACE(V(b), 1/(1+s), M=3)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<BehavioralVoltageSource>(model.Circuit["BMIX"]);
            Assert.True(EqualsWithTol(5.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_BVoltageLaplaceFunctionUsesArbitraryInput_Expect_InputHelperAndDoubledGain()
        {
            var model = GetSpiceSharpModel(
                "B V LAPLACE arbitrary input OP",
                "VIN in 0 1",
                "BLOW out 0 V={LAPLACE(2*V(in), 1/(1+s))}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<BehavioralVoltageSource>(model.Circuit["__ssp_laplace_input_BLOW_0_src"]);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["BLOW"]);
            Assert.True(EqualsWithTol(2.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_MixedDelayedLaplaceFunctionsRunTransient_Expect_NoRuntimeException()
        {
            var model = GetSpiceSharpModel(
                "B mixed delayed LAPLACE TRAN",
                "VA a 0 PULSE(0 1 0 1n 1n 100u 200u)",
                "VB b 0 PULSE(0 1 0 1n 1n 100u 200u)",
                "BMIX out 0 V={LAPLACE(V(a), 1/(1+s*1u), TD=1u) + LAPLACE(V(b), 1/(1+s*1u), DELAY=2u)}",
                "RLOAD out 0 1k",
                ".TRAN 50n 8u",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            var exception = Record.Exception(() => RunTransientSimulation(model, "V(out)"));
            Assert.Null(exception);
        }

        [Fact]
        public void When_HLaplaceIsUsed_Expect_ReaderValidationError()
        {
            var model = GetSpiceSharpModel(
                "H LAPLACE invalid voltage input",
                "VIN in 0 1",
                "HBAD out 0 LAPLACE {V(in)} = {1/(1+s)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertReaderErrorContains(model, "I(source)");
        }

        [Fact]
        public void When_ELaplace2ndOrderButterworthLowPassRunsOp_Expect_UnitDcGain()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order Butterworth LP OP",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 1",
                "ELP2 out 0 LAPLACE {V(in)} = {wc^2/(s^2+sqrt(2)*wc*s+wc^2)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["ELP2"]);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(1.0, export), $"Expected 2nd-order Butterworth LP DC gain near 1, got {export}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderButterworthLowPassRunsAc_Expect_MagnitudePhaseAndRolloff()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order Butterworth LP AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "ELP2 out 0 LAPLACE {V(in)} = {wc^2/(s^2+sqrt(2)*wc*s+wc^2)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_pass FIND VM(out) AT=10",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vm_10fc FIND VM(out) AT=10k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_pass");
            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vm_10fc");
            AssertMeasurementSuccess(model, "vp_fc");

            double vmPass = model.Measurements["vm_pass"][0].Value;
            double vmFc = model.Measurements["vm_fc"][0].Value;
            double vm10Fc = model.Measurements["vm_10fc"][0].Value;
            double vpFc = model.Measurements["vp_fc"][0].Value;

            // Butterworth: |H(f)| = 1/sqrt(1+(f/fc)^(2n)); n=2, f=10fc -> 1/sqrt(10001) ~= 0.01
            Assert.True(Math.Abs(vmPass - 1.0) < 0.01, $"Expected passband magnitude near 1 at 10Hz, got {vmPass}.");
            Assert.True(Math.Abs(vmFc - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected -3dB magnitude near 0.707 at cutoff, got {vmFc}.");
            Assert.True(vm10Fc < 0.02, $"Expected magnitude < 0.02 one decade above cutoff (-40dB/dec), got {vm10Fc}.");
            // At fc: H(jwc) = wc^2/(j*sqrt(2)*wc^2) = 1/(j*sqrt(2)), phase = -pi/2
            Assert.True(Math.Abs(vpFc - (-Math.PI / 2.0)) < 0.1, $"Expected phase near -pi/2 at cutoff, got {vpFc}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderButterworthHighPassRunsOp_Expect_ZeroDcGain()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order Butterworth HP OP",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 1",
                "EHP2 out 0 LAPLACE {V(in)} = {s^2/(s^2+sqrt(2)*wc*s+wc^2)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            // H(0) = 0^2/(0^2+...) = 0
            Assert.True(Math.Abs(export) < 1e-6, $"Expected 2nd-order Butterworth HP DC gain near 0, got {export}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderButterworthHighPassRunsAc_Expect_MagnitudeAndPhaseAtCutoff()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order Butterworth HP AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "EHP2 out 0 LAPLACE {V(in)} = {s^2/(s^2+sqrt(2)*wc*s+wc^2)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_100 FIND VM(out) AT=100",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_100");
            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vp_fc");

            double vm100 = model.Measurements["vm_100"][0].Value;
            double vmFc = model.Measurements["vm_fc"][0].Value;
            double vpFc = model.Measurements["vp_fc"][0].Value;

            Assert.True(vm100 < 0.02, $"Expected stop-band magnitude < 0.02 one decade below cutoff, got {vm100}.");
            Assert.True(Math.Abs(vmFc - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected -3dB magnitude near 0.707 at cutoff, got {vmFc}.");
            // At fc: H(jwc) = -wc^2/(j*sqrt(2)*wc^2) = -1/(j*sqrt(2)) = j/sqrt(2), phase = +pi/2
            Assert.True(Math.Abs(vpFc - (Math.PI / 2.0)) < 0.1, $"Expected phase near +pi/2 at cutoff for 2nd-order HP, got {vpFc}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderBandPassRunsOp_Expect_ZeroDcGain()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order band-pass OP",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                ".PARAM Q=1",
                "VIN in 0 1",
                "EBP out 0 LAPLACE {V(in)} = {(wc/Q)*s/(s^2+(wc/Q)*s+wc^2)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            // H(0) = (wc/Q)*0/(wc^2) = 0
            Assert.True(Math.Abs(export) < 1e-6, $"Expected band-pass DC gain near 0, got {export}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderBandPassRunsAc_Expect_UnitPeakGainAndRolloff()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order band-pass AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                ".PARAM Q=1",
                "VIN in 0 AC 1",
                "EBP out 0 LAPLACE {V(in)} = {(wc/Q)*s/(s^2+(wc/Q)*s+wc^2)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_100 FIND VM(out) AT=100",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vm_10k FIND VM(out) AT=10k",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_100");
            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vm_10k");

            double vm100 = model.Measurements["vm_100"][0].Value;
            double vmFc = model.Measurements["vm_fc"][0].Value;
            double vm10k = model.Measurements["vm_10k"][0].Value;

            // At resonance: H(jwc) = (wc/Q)*jwc/(j*wc^2/Q) = 1.0 (exact)
            Assert.True(Math.Abs(vmFc - 1.0) < 0.05, $"Expected band-pass peak gain near 1 at resonance, got {vmFc}.");
            // One decade away from resonance each stage rolls off ~20dB for Q=1
            Assert.True(vm100 < 0.15, $"Expected magnitude < 0.15 one decade below resonance, got {vm100}.");
            Assert.True(vm10k < 0.15, $"Expected magnitude < 0.15 one decade above resonance, got {vm10k}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderNotchRunsOp_Expect_UnitDcGain()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order notch OP",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                ".PARAM Q=10",
                "VIN in 0 1",
                "ENOTCH out 0 LAPLACE {V(in)} = {(s^2+wc^2)/(s^2+(wc/Q)*s+wc^2)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            // H(0) = wc^2/wc^2 = 1
            Assert.True(EqualsWithTol(1.0, export), $"Expected notch filter DC gain near 1, got {export}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderNotchRunsAc_Expect_DeepAttenuationAtNotchAndFullPassband()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order notch AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                ".PARAM Q=10",
                "VIN in 0 AC 1",
                "ENOTCH out 0 LAPLACE {V(in)} = {(s^2+wc^2)/(s^2+(wc/Q)*s+wc^2)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_100 FIND VM(out) AT=100",
                ".MEAS AC vm_notch FIND VM(out) AT=1k",
                ".MEAS AC vm_10k FIND VM(out) AT=10k",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_100");
            AssertMeasurementSuccess(model, "vm_notch");
            AssertMeasurementSuccess(model, "vm_10k");

            double vm100 = model.Measurements["vm_100"][0].Value;
            double vmNotch = model.Measurements["vm_notch"][0].Value;
            double vm10k = model.Measurements["vm_10k"][0].Value;

            // At notch: numerator (jwc)^2+wc^2 = -wc^2+wc^2 = 0, so H(jwc) = 0
            Assert.True(vmNotch < 0.01, $"Expected near-zero magnitude at notch frequency, got {vmNotch}.");
            Assert.True(Math.Abs(vm100 - 1.0) < 0.02, $"Expected passband magnitude near 1 below the notch, got {vm100}.");
            Assert.True(Math.Abs(vm10k - 1.0) < 0.02, $"Expected passband magnitude near 1 above the notch, got {vm10k}.");
        }

        [Fact]
        public void When_ELaplace3rdOrderButterworthLowPassRunsOp_Expect_UnitDcGain()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 3rd-order Butterworth LP OP",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 1",
                "ELP3 out 0 LAPLACE {V(in)} = {wc^3/(s^3+2*wc*s^2+2*wc^2*s+wc^3)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["ELP3"]);

            double export = RunOpSimulation(model, "V(out)");
            Assert.True(EqualsWithTol(1.0, export), $"Expected 3rd-order Butterworth LP DC gain near 1, got {export}.");
        }

        [Fact]
        public void When_ELaplace3rdOrderButterworthLowPassRunsAc_Expect_MagnitudePhaseAnd60DbDecRolloff()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 3rd-order Butterworth LP AC",
                ".PARAM fc=1k",
                ".PARAM wc={2*PI*fc}",
                "VIN in 0 AC 1",
                "ELP3 out 0 LAPLACE {V(in)} = {wc^3/(s^3+2*wc*s^2+2*wc^2*s+wc^3)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".MEAS AC vm_10fc FIND VM(out) AT=10k",
                ".MEAS AC vp_fc FIND VP(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_fc");
            AssertMeasurementSuccess(model, "vm_10fc");
            AssertMeasurementSuccess(model, "vp_fc");

            double vmFc = model.Measurements["vm_fc"][0].Value;
            double vm10Fc = model.Measurements["vm_10fc"][0].Value;
            double vpFc = model.Measurements["vp_fc"][0].Value;

            // Butterworth n=3: |H(fc)| = 1/sqrt(2), |H(10fc)| = 1/sqrt(1+10^6) ~= 0.001
            Assert.True(Math.Abs(vmFc - (1.0 / Math.Sqrt(2.0))) < 0.03, $"Expected -3dB magnitude near 0.707 at cutoff, got {vmFc}.");
            Assert.True(vm10Fc < 0.002, $"Expected magnitude < 0.002 one decade above cutoff (-60dB/dec), got {vm10Fc}.");
            // At fc: H(jwc) = wc^3/(-wc^3-2wc^3+j2wc^3+wc^3) = 1/(-1+j), phase = -3*pi/4
            Assert.True(Math.Abs(vpFc - (-3.0 * Math.PI / 4.0)) < 0.1, $"Expected phase near -3pi/4 (-135deg) at cutoff for 3rd-order Butterworth, got {vpFc}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderLowPassWithHighQRunsAc_Expect_ResonantPeakEqualsQ()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE 2nd-order resonant LP AC",
                ".PARAM fn=1k",
                ".PARAM wn={2*PI*fn}",
                ".PARAM Q=5",
                "VIN in 0 AC 1",
                "ERES out 0 LAPLACE {V(in)} = {wn^2/(s^2+wn/Q*s+wn^2)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_peak FIND VM(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_peak");
            double vmPeak = model.Measurements["vm_peak"][0].Value;

            // H(jwn) = wn^2/(-wn^2+j*wn^2/Q+wn^2) = Q/(j) -> |H(jwn)| = Q = 5
            Assert.True(Math.Abs(vmPeak - 5.0) < 0.3, $"Expected resonant peak magnitude near Q=5, got {vmPeak}.");
        }

        [Fact]
        public void When_ELaplaceLeadCompensatorRunsOp_Expect_UnitDcGain()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE lead compensator OP",
                ".PARAM tz=100u",
                ".PARAM tp=10u",
                "VIN in 0 1",
                "ELEAD out 0 LAPLACE {V(in)} = {(1+s*tz)/(1+s*tp)}",
                "RLOAD out 0 1k",
                ".OP",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            double export = RunOpSimulation(model, "V(out)");
            // H(0) = (1+0)/(1+0) = 1
            Assert.True(EqualsWithTol(1.0, export), $"Expected lead compensator DC gain near 1, got {export}.");
        }

        [Fact]
        public void When_ELaplaceLeadCompensatorRunsAc_Expect_HfGainEqualsTzOverTp()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE lead compensator AC",
                ".PARAM tz=100u",
                ".PARAM tp=10u",
                "VIN in 0 AC 1",
                "ELEAD out 0 LAPLACE {V(in)} = {(1+s*tz)/(1+s*tp)}",
                "RLOAD out 0 1k",
                ".AC DEC 20 1 10Meg",
                ".MEAS AC vm_lf FIND VM(out) AT=1",
                ".MEAS AC vm_hf FIND VM(out) AT=1Meg",
                ".MEAS AC vp_gm FIND VP(out) AT=5031",
                ".END");

            AssertNoValidationErrors(model);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_lf");
            AssertMeasurementSuccess(model, "vm_hf");
            AssertMeasurementSuccess(model, "vp_gm");

            double vmLf = model.Measurements["vm_lf"][0].Value;
            double vmHf = model.Measurements["vm_hf"][0].Value;
            double vpGm = model.Measurements["vp_gm"][0].Value;

            // H(s->0) = 1, H(s->inf) = tz/tp = 100u/10u = 10
            Assert.True(Math.Abs(vmLf - 1.0) < 0.02, $"Expected LF gain near 1, got {vmLf}.");
            Assert.True(Math.Abs(vmHf - 10.0) < 0.5, $"Expected HF gain near tz/tp=10, got {vmHf}.");
            // At geometric mean of zero (1591Hz) and pole (15915Hz): fg = sqrt(1591*15915) ~= 5031Hz
            // Phase lead = arctan(wg*tz) - arctan(wg*tp) ~= 54.9deg ~= 0.958rad > 0
            Assert.True(vpGm > 0.5, $"Expected positive phase lead (~0.96rad) at geometric mean frequency 5031Hz, got {vpGm}.");
        }

        [Fact]
        public void When_TwoELaplaceLowPassFiltersAreCascaded_Expect_HalvedMagnitudeAtCutoff()
        {
            var model = GetSpiceSharpModel(
                "E LAPLACE cascaded LP AC",
                ".PARAM fc=1k",
                ".PARAM tau={1/(2*PI*fc)}",
                "VIN in 0 AC 1",
                "ELOW1 mid 0 LAPLACE {V(in)} = {1/(1+s*tau)}",
                "RMID mid 0 1k",
                "ELOW2 out 0 LAPLACE {V(mid)} = {1/(1+s*tau)}",
                "RLOAD out 0 1k",
                ".AC DEC 30 10 100k",
                ".MEAS AC vm_lf FIND VM(out) AT=10",
                ".MEAS AC vm_fc FIND VM(out) AT=1k",
                ".END");

            AssertNoValidationErrors(model);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["ELOW1"]);
            Assert.IsType<LaplaceVoltageControlledVoltageSource>(model.Circuit["ELOW2"]);
            RunSimulations(model);

            AssertMeasurementSuccess(model, "vm_lf");
            AssertMeasurementSuccess(model, "vm_fc");

            double vmLf = model.Measurements["vm_lf"][0].Value;
            double vmFc = model.Measurements["vm_fc"][0].Value;

            Assert.True(Math.Abs(vmLf - 1.0) < 0.02, $"Expected cascaded LP passband gain near 1, got {vmLf}.");
            // Each stage: |H(fc)| = 1/sqrt(2); combined: (1/sqrt(2))^2 = 0.5
            Assert.True(Math.Abs(vmFc - 0.5) < 0.04, $"Expected cascaded LP magnitude near 0.5 at shared cutoff, got {vmFc}.");
        }

        [Fact]
        public void When_ELaplace2ndOrderUnderdampedRunsTransient_Expect_OvershootWithinAnalyticalBounds()
        {
            // zeta=0.3, fn=10kHz: peak time = pi/(wn*sqrt(1-zeta^2)) ~= 52.4us
            // overshoot = exp(-pi*zeta/sqrt(1-zeta^2)) ~= 37.3% -> peak ~= 1.373
            var model = GetSpiceSharpModel(
                "E LAPLACE underdamped 2nd-order TRAN",
                ".PARAM fn=10k",
                ".PARAM wn={2*PI*fn}",
                ".PARAM zeta=0.3",
                "VIN in 0 PULSE(0 1 0 1n 1n 1m 2m)",
                "EUNDERDAMP out 0 LAPLACE {V(in)} = {wn^2/(s^2+2*zeta*wn*s+wn^2)}",
                "RLOAD out 0 1k",
                ".TRAN 1u 300u",
                ".SAVE V(out)",
                ".END");

            AssertNoValidationErrors(model);

            var exports = RunTransientSimulation(model, "V(out)");

            AssertTransientPoint(exports, 52.4e-6, 1.373, 0.12);
            AssertTransientPoint(exports, 200e-6, 1.0, 0.15);
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
