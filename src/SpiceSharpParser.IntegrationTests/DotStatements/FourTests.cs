using System;
using System.Linq;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Fourier;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.DotStatements
{
    public class FourTests : BaseTests
    {
        [Fact]
        public void PureSineCreatesFourierAnalysis()
        {
            var model = GetSpiceSharpModel(
                "FOUR pure sine",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 1k V(OUT)",
                ".END");

            RunSimulations(model);

            AssertNoValidationErrors(model);
            var result = AssertSingleSuccessfulResult(model);
            Assert.Equal("V(OUT)", result.SignalName);
            Assert.False(string.IsNullOrWhiteSpace(result.SimulationName));
            Assert.Equal(10, result.Harmonics.Count);
            Assert.Equal(0.0, Harmonic(result, 0).Frequency);
            Assert.Equal(9000.0, Harmonic(result, 9).Frequency);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.95, 1.05);
            Assert.InRange(Harmonic(result, 1).NormalizedMagnitude, 0.99, 1.01);
            Assert.InRange(Harmonic(result, 1).NormalizedMagnitudeDecibels, -0.1, 0.1);
            Assert.InRange(Math.Abs(result.TotalHarmonicDistortionPercent), 0.0, 1.0);
        }

        [Fact]
        public void SecondHarmonicReportsExpectedThd()
        {
            var model = GetSpiceSharpModel(
                "FOUR second harmonic",
                "V1 A 0 SIN(0 1 1k)",
                "V2 A OUT SIN(0 0.1 2k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 1k V(OUT)",
                ".END");

            RunSimulations(model);

            AssertNoValidationErrors(model);
            var result = AssertSingleSuccessfulResult(model);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.95, 1.05);
            Assert.InRange(Harmonic(result, 2).Magnitude, 0.08, 0.12);
            Assert.InRange(result.TotalHarmonicDistortionPercent, 8.0, 12.0);
        }

        [Fact]
        public void MultipleSignalsProduceOneResultPerSignal()
        {
            var model = GetSpiceSharpModel(
                "FOUR multiple signals",
                "V1 IN 0 SIN(0 1 1k)",
                "R1 IN OUT 1k",
                "R2 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 1k V(IN) V(OUT)",
                ".END");

            RunSimulations(model);

            AssertNoValidationErrors(model);
            Assert.Equal(2, model.FourierAnalyses.Count);
            Assert.Contains(model.FourierAnalyses, r => r.SignalName == "V(IN)" && r.Success);
            Assert.Contains(model.FourierAnalyses, r => r.SignalName == "V(OUT)" && r.Success);
        }

        [Fact]
        public void CurrentSignalExpressionIsAnalyzed()
        {
            var model = GetSpiceSharpModel(
                "FOUR current signal",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 1k I(V1)",
                ".END");

            RunSimulations(model);

            AssertNoValidationErrors(model);
            var result = AssertSingleSuccessfulResult(model);
            Assert.Equal("I(V1)", result.SignalName);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.0009, 0.0011);
        }

        [Fact]
        public void ParameterizedFundamentalFrequencyIsEvaluated()
        {
            var model = GetSpiceSharpModel(
                "FOUR parameter frequency",
                ".PARAM freq=1k",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR {freq} V(OUT)",
                ".END");

            RunSimulations(model);

            AssertNoValidationErrors(model);
            var result = AssertSingleSuccessfulResult(model);
            Assert.Equal(1000.0, result.FundamentalFrequency, 6);
            Assert.InRange(Harmonic(result, 1).Magnitude, 0.95, 1.05);
        }

        [Fact]
        public void StepProducesOneResultPerTransientSimulation()
        {
            var model = GetSpiceSharpModel(
                "FOUR step",
                ".PARAM amp=1",
                "V1 OUT 0 SIN(0 {amp} 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".STEP PARAM amp LIST 1 2 3",
                ".FOUR 1k V(OUT)",
                ".END");

            RunSimulations(model);

            AssertNoValidationErrors(model);
            Assert.Equal(3, model.FourierAnalyses.Count);
            Assert.All(model.FourierAnalyses, result => Assert.True(result.Success));
            Assert.All(model.FourierAnalyses, result => Assert.InRange(Harmonic(result, 1).Magnitude, 0.9, 1.1));
        }

        [Fact]
        public void NoTransientAnalysisProducesValidationError()
        {
            var model = GetSpiceSharpModel(
                "FOUR no tran",
                "V1 OUT 0 1",
                "R1 OUT 0 1k",
                ".OP",
                ".FOUR 1k V(OUT)",
                ".END");

            AssertValidationContains(model, ".FOUR requires a .TRAN analysis");
        }

        [Fact]
        public void BadFrequencyProducesValidationError()
        {
            var model = GetSpiceSharpModel(
                "FOUR bad frequency",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 0 V(OUT)",
                ".END");

            AssertValidationContains(model, "fundamental frequency must be positive");
        }

        [Fact]
        public void MissingSignalOperandProducesValidationError()
        {
            var model = GetSpiceSharpModel(
                "FOUR missing operand",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 1k",
                ".END");

            AssertValidationContains(model, "requires a fundamental frequency and at least one signal");
            Assert.Empty(model.FourierAnalyses);
        }

        [Fact]
        public void TooShortTransientProducesFailedResult()
        {
            var model = GetSpiceSharpModel(
                "FOUR too short",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 0.5m 0 2u",
                ".FOUR 1k V(OUT)",
                ".END");

            RunSimulations(model);

            AssertValidationContains(model, "complete final period");
            Assert.Single(model.FourierAnalyses);
            Assert.False(model.FourierAnalyses[0].Success);
            Assert.True(double.IsNaN(model.FourierAnalyses[0].TotalHarmonicDistortionPercent));
        }

        [Fact]
        public void MissingSignalProducesValidationErrorAfterRun()
        {
            var model = GetSpiceSharpModel(
                "FOUR missing signal",
                "V1 OUT 0 SIN(0 1 1k)",
                "R1 OUT 0 1k",
                ".TRAN 1u 10m 0 2u",
                ".FOUR 1k V(MISSING)",
                ".END");

            RunSimulations(model);

            AssertValidationContains(model, ".FOUR V(MISSING)");
            Assert.Single(model.FourierAnalyses);
            Assert.False(model.FourierAnalyses[0].Success);
        }

        private static FourierAnalysisResult AssertSingleSuccessfulResult(SpiceSharpModel model)
        {
            Assert.Single(model.FourierAnalyses);
            var result = model.FourierAnalyses[0];
            Assert.True(result.Success, result.ErrorMessage);
            return result;
        }

        private static FourierHarmonic Harmonic(FourierAnalysisResult result, int harmonic)
        {
            return result.Harmonics.Single(h => h.HarmonicNumber == harmonic);
        }

        private static void AssertNoValidationErrors(SpiceSharpModel model)
        {
            string messages = string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message));
            Assert.False(model.ValidationResult.HasError, messages);
        }

        private static void AssertValidationContains(SpiceSharpModel model, string expectedText)
        {
            Assert.True(model.ValidationResult.HasError);
            string messages = string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message));
            Assert.Contains(expectedText, messages, StringComparison.OrdinalIgnoreCase);
        }
    }
}
