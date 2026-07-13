using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave;
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
                "V1 out 0 VALUE={arccos(1)+arcsin(1)+arctan(1)+fabs(-3)+sgn(-2)+round(1.6)+pwr(2,3)+pwrs(-2,3)+hypot(3,4)+table(1.5,0,0,1,10,2,20)+tbl(1.5,0,0,1,10,2,20)+uplim(2,1,0.2)+dnlim(0,1,0.2)+2**3}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(49.0 + (0.75 * Math.PI), RunOpSimulation(model, "V(out)")));
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
        [MemberData(nameof(PwlFileFormatCases))]
        public void When_LtspicePwlFileUsesSupportedTextVariant_Expect_TransientReferenceValues(string fileContent)
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                File.WriteAllText(Path.Combine(tempDirectory, "shape.txt"), fileContent);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - PWL text variants",
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

        [Fact]
        public void When_LtspicePwlScaleFactorsAreRead_Expect_ScaledTransientReferenceValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - PWL scale factors",
                ".param ts=2 vs=3",
                "V1 out 0 PWL VALUE_SCALE_FACTOR={vs} TIME_SCALE_FACTOR={ts} (0,1,1,2,2,0)",
                "R1 out 0 1k",
                ".tran 0.25 4",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlScaledReference));
        }

        [Fact]
        public void When_LtspicePwlScaleFactorsAreRepeated_Expect_LastAssignmentWins()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - duplicate PWL scale factors",
                "V1 out 0 PWL TIME_SCALE_FACTOR=0 VALUE_SCALE_FACTOR=1e309 TIME_SCALE_FACTOR=4 VALUE_SCALE_FACTOR=-3 (0,0,1,1,2,0)",
                "R1 out 0 1k",
                ".tran 0.5 8",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlLastScaleFactorReference));
        }

        [Fact]
        public void When_LtspiceCurrentPwlHasScaleFactors_Expect_ScaledWaveformPoints()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - current PWL scale factors",
                "I1 out 0 PWL TIME_SCALE_FACTOR=2 VALUE_SCALE_FACTOR=-3 (0,1,1,2)",
                "R1 out 0 1k",
                ".tran 0.25 2",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            var source = Assert.IsType<CurrentSource>(model.Circuit["I1"]);
            var waveform = Assert.IsType<Pwl>(source.Parameters.Waveform);
            Assert.Collection(
                waveform.Points,
                point => Assert.Equal((0.0, -3.0), (point.Time, point.Value)),
                point => Assert.Equal((2.0, -6.0), (point.Time, point.Value)));
        }

        [Fact]
        public void When_LtspicePwlFileHasScaleFactors_Expect_ScaledFilePoints()
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                File.WriteAllText(
                    Path.Combine(tempDirectory, "shape.txt"),
                    "0,1" + Environment.NewLine
                    + "1,2" + Environment.NewLine
                    + "2,0" + Environment.NewLine);

                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - scaled PWL file",
                    "V1 out 0 PWL TIME_SCALE_FACTOR=2 VALUE_SCALE_FACTOR=3 file=\"shape.txt\"",
                    "R1 out 0 1k",
                    ".tran 0.25 4",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);

                var exports = RunTransientSimulation(model, "V(out)");
                Assert.NotEmpty(exports);
                Assert.True(EqualsWithTol(exports, PwlScaledReference));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Fact]
        public void When_LtspicePwlRepeatForIsRead_Expect_ExpandedTransientReferenceValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - PWL repeat for",
                "V1 out 0 PWL REPEAT FOR 3 (1n,1,3n,3) ENDREPEAT",
                "R1 out 0 1k",
                ".tran 0.5n 9.5n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlRepeatForReference));
        }

        [Fact]
        public void When_LtspicePwlRepeatForeverIsRead_Expect_RepeatingTransientReferenceValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - PWL repeat forever",
                "V1 out 0 PWL REPEAT FOREVER (0,0,1n,1,2n,0) ENDREPEAT",
                "R1 out 0 1k",
                ".tran 0.5n 5n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlRepeatTriangleReference));
        }

        [Fact]
        public void When_LtspicePwlRepeatForUsesRelativeTimes_Expect_ExpandedTransientReferenceValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - PWL repeat for relative times",
                "V1 out 0 PWL REPEAT FOR 2 (0,0,+1n,1,+1n,0) ENDREPEAT",
                "R1 out 0 1k",
                ".tran 0.5n 4.5n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlFiniteRepeatTriangleReference));
        }

        [Fact]
        public void When_LtspicePwlRepeatForeverUsesRelativeTimes_Expect_RepeatingTransientReferenceValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - PWL repeat forever relative times",
                "V1 out 0 PWL REPEAT FOREVER (0,0,+1n,1,+1n,0) ENDREPEAT",
                "R1 out 0 1k",
                ".tran 0.5n 5n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlRepeatTriangleReference));
        }

        [Fact]
        public void When_LtspicePwlRepeatUsesScaleFactors_Expect_ScaledPeriodAndValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - scaled PWL repeat",
                "V1 out 0 PWL TIME_SCALE_FACTOR=2 VALUE_SCALE_FACTOR=4 REPEAT FOR 2 (0,0,+1,1,+1,0) ENDREPEAT",
                "R1 out 0 1k",
                ".tran 0.5 8",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlScaledRepeatReference));
        }

        [Theory]
        [InlineData("V1 out 0 PWL REPEAT FOR 2 (0,0,1n,1,2n,0) ENDREPEAT")]
        [InlineData("V1 out 0 PWL REPEAT for=2 (0,0,1n,1,2n,0) ENDREPEAT")]
        [InlineData("V1 out 0 PWL repeat=2 (0,0,1n,1,2n,0) endrepeat")]
        public void When_LtspicePwlRepeatCountVariantIsRead_Expect_FiniteRepeat(string sourceLine)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - PWL repeat count variant",
                sourceLine,
                "R1 out 0 1k",
                ".tran 0.5n 5n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, PwlFiniteRepeatTriangleReference));
        }

        [Theory]
        [InlineData("V1 out 0 PWL file=\"missing.txt\"", "does not exist")]
        [InlineData("V1 out 0 PWL repeat 0 0 1n 1 endrepeat", "FOR")]
        public void When_LtspicePwlFileOrBareRepeatSyntaxIsInvalid_Expect_TargetedError(string sourceLine, string expectedMessage)
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
        [InlineData("V1 out 0 PWL REPEAT FOR 2 (0,0,1n,0)", "ENDREPEAT")]
        [InlineData("V1 out 0 PWL REPEAT FOR 0 (0,0,1n,0) ENDREPEAT", "repeat count")]
        [InlineData("V1 out 0 PWL REPEAT FOR 2 (0,0,1n) ENDREPEAT", "time/value")]
        [InlineData("V1 out 0 PWL REPEAT FOR 2 (0,0,0,0) ENDREPEAT", "increasing")]
        [InlineData("V1 out 0 PWL REPEAT FOR 2 (0,0,1n,1) ENDREPEAT", "contradictory")]
        [InlineData("V1 out 0 PWL REPEAT FOR 2 (0,0,+1e309,0) ENDREPEAT", "non-finite")]
        [InlineData("V1 out 0 PWL ENDREPEAT", "requires")]
        public void When_LtspicePwlRepeatSyntaxIsMalformed_Expect_TargetedError(string sourceLine, string expectedMessage)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - malformed PWL repeat",
                sourceLine,
                "R1 out 0 1k",
                ".tran 1n 6n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "PWL");
            AssertErrorContains(model.ValidationResult, expectedMessage);
        }

        [Theory]
        [InlineData("V1 out 0 PWL TIME_SCALE_FACTOR=0 (0,0,1,1)", "positive")]
        [InlineData("V1 out 0 PWL TIME_SCALE_FACTOR=-1 (0,0,1,1)", "positive")]
        [InlineData("V1 out 0 PWL VALUE_SCALE_FACTOR=1e309 (0,0,1,1)", "finite")]
        [InlineData("V1 out 0 PWL (0,0,1,1) TIME_SCALE_FACTOR=2", "precede")]
        [InlineData("V1 out 0 PWL TIME_SCALE_FACTOR=1e308 (0,0,10,1)", "non-finite")]
        public void When_LtspicePwlScaleFactorIsInvalid_Expect_TargetedError(
            string sourceLine,
            string expectedMessage)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - invalid PWL scale factor",
                sourceLine,
                "R1 out 0 1k",
                ".tran 0.1 2",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "PWL");
            AssertErrorContains(model.ValidationResult, expectedMessage);
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
        [InlineData("V1 out 0 1 Rser=10")]
        [InlineData("V1 out 0 1 R=10")]
        [InlineData("V1 out 0 1 Rser 10")]
        public void When_LtspiceVoltageSourceSeriesResistanceIsRead_Expect_SynthesizedDivider(string sourceLine)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - voltage source series resistance",
                sourceLine,
                "R1 out 0 90",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(0.9, RunOpSimulation(model, "V(out)")));
        }

        [Theory]
        [InlineData("I1 out 0 1 load=1")]
        [InlineData("I1 out 0 1 R=1")]
        [InlineData("I1 out 0 1 load 1")]
        public void When_LtspiceCurrentSourceLoadResistanceIsRead_Expect_SynthesizedShunt(string sourceLine)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - current source load resistance",
                sourceLine,
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(-1000.0 / 1001.0, RunOpSimulation(model, "V(out)")));
        }

        [Theory]
        [InlineData("V1 out 0 1 Cpar=1p", "V1_cpar")]
        [InlineData("I1 out 0 1 Cpar=1p", "I1_cpar")]
        public void When_LtspiceSourceParallelCapacitanceIsRead_Expect_SynthesizedCapacitor(
            string sourceLine,
            string expectedCapacitorName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - source parallel capacitance",
                sourceLine,
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.Contains(model.Circuit, entity => entity.Name == expectedCapacitorName);
        }

        [Fact]
        public void When_LtspiceBehavioralVoltageSourceHasMultipleTopologyOptions_Expect_AllHelpersAreSynthesized()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - behavioral source topology options",
                "VCTRL ctrl 0 1",
                "V1 out 0 VALUE={2*V(ctrl)} Rser=100 load=300 Cpar=1p",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(1.5, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "V1_rser");
            Assert.Contains(model.Circuit, entity => entity.Name == "V1_load");
            Assert.Contains(model.Circuit, entity => entity.Name == "V1_cpar");
        }

        [Fact]
        public void When_LtspiceTableVoltageSourceHasSeriesResistance_Expect_TableAndTopologyLowering()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - table source topology options",
                "V1 out 0 tbl=(0.5,0,0,1,10) R=10",
                "R1 out 0 90",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(4.5, RunOpSimulation(model, "V(out)")));
            Assert.Contains(model.Circuit, entity => entity.Name == "V1_rser");
        }

        [Fact]
        public void When_LtspiceCurrentSourceHasLoadAndCpar_Expect_RcTransient()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - current source load and Cpar transient",
                "I1 out 0 PULSE(0 -1m 1n 1n 1n 10u 20u) load=1k Cpar=1n",
                ".tran 100n 5u",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);

            var oneTau = exports.OrderBy(export => Math.Abs(export.Item1 - 1e-6)).First();
            Assert.InRange(oneTau.Item2, 0.55, 0.72);
            Assert.InRange(exports.Last().Item2, 0.98, 1.01);
        }

        [Fact]
        public void When_LtspiceSourceResistanceIsInsideRepeatedSubcircuits_Expect_InternalNodesAreScoped()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - subcircuit source topology options",
                "XU1 out1 0 sourcecell",
                "XU2 out2 0 sourcecell",
                "R1 out1 0 90",
                "R2 out2 0 40",
                ".subckt sourcecell p n",
                "V1 p n 1 Rser=10",
                ".ends sourcecell",
                ".op",
                ".save V(out1)",
                ".save V(out2)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunOpSimulation(model, "V(out1)", "V(out2)");
            Assert.True(EqualsWithTol(0.9, exports[0]));
            Assert.True(EqualsWithTol(0.8, exports[1]));
        }

        [Theory]
        [InlineData("V1 out 0 1 Rser", "Rser")]
        [InlineData("I1 out 0 1 load", "load")]
        public void When_LtspiceSourceTopologyOptionOmitsValue_Expect_TargetedError(
            string sourceLine,
            string optionName)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - invalid source topology option",
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
        [InlineData("V1 out 0 wavefile=\"missing.wav\"", "does not exist")]
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
        public void When_LtspiceWaveFileOmitsChannel_Expect_ChannelZero()
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                WriteMonoWaveFile(Path.Combine(tempDirectory, "input.wav"));
                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - default wavefile channel",
                    "V1 out 0 wavefile=\"input.wav\"",
                    "R1 out 0 1k",
                    ".tran 1m 2m",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);
                var source = Assert.IsType<VoltageSource>(model.Circuit["V1"]);
                var waveform = Assert.IsType<Wave>(source.Parameters.Waveform);
                Assert.Equal(0, waveform.Channel);
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Fact]
        public void When_LtspiceWaveFileSpecifiesChannel_Expect_ExplicitChannel()
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                WriteStereoWaveFile(Path.Combine(tempDirectory, "input.wav"));
                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.LTspice,
                    "LTspice P2 - explicit wavefile channel",
                    "V1 out 0 wavefile=\"input.wav\" chan=1",
                    "R1 out 0 1k",
                    ".tran 1m 2m",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(model.ValidationResult);
                var source = Assert.IsType<VoltageSource>(model.Circuit["V1"]);
                var waveform = Assert.IsType<Wave>(source.Parameters.Waveform);
                Assert.Equal(1, waveform.Channel);
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Fact]
        public void When_DefaultWaveFileOmitsChannel_Expect_TargetedError()
        {
            var tempDirectory = CreateTempDirectory();

            try
            {
                WriteMonoWaveFile(Path.Combine(tempDirectory, "input.wav"));
                var model = GetSpiceSharpModelWithCompatibilityAndWorkingDirectory(
                    tempDirectory,
                    CompatibilityOptions.None,
                    "Default mode - missing wavefile channel",
                    "V1 out 0 wavefile=\"input.wav\"",
                    "R1 out 0 1k",
                    ".tran 1m 2m",
                    ".save V(out)",
                    ".end");

                Assert.True(model.ValidationResult.HasError);
                AssertErrorContains(model.ValidationResult, "wavefile");
                AssertErrorContains(model.ValidationResult, "chan");
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
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

        [Fact]
        public void When_LtspiceSineHasCycleCount_Expect_TransientStopsAfterFiniteCycles()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - finite-cycle SINE",
                "V1 out 0 SINE(1 2 250Meg 1n 0 0 2)",
                "R1 out 0 1k",
                ".tran 1n 12n",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);

            var exports = RunTransientSimulation(model, "V(out)");
            Assert.NotEmpty(exports);
            Assert.True(EqualsWithTol(exports, FiniteSineReference));
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
        [InlineData("V1 out 0 SINE(0 1 0 0 0 0 3)", "frequency")]
        [InlineData("V1 out 0 SINE(0 1 1k 0 0 0 0)", "cycle-count")]
        public void When_LtspiceSineCycleCountArgumentsAreInvalid_Expect_TargetedError(
            string sourceLine,
            string expectedMessage)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - invalid finite-cycle SINE",
                sourceLine,
                "R1 out 0 1k",
                ".tran 1n 50n",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            AssertErrorContains(model.ValidationResult, "SINE");
            AssertErrorContains(model.ValidationResult, expectedMessage);
        }

        [Theory]
        [InlineData("uplim(V(in),1,0.2)", 0.9995042495646668)]
        [InlineData("dnlim(V(in)-2,1,0.2)", 1.0004957504353332)]
        public void When_LtspiceSmoothLimitExpressionIsRead_Expect_OpReferenceValue(
            string expression,
            double expected)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - smooth limit expression",
                "VIN in 0 2",
                "B1 out 0 V={" + expression + "}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(expected, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_LtspiceBehavioralRandomFunctionsAreRead_Expect_DeterministicTransientShapes()
        {
            var fineModel = CreateBehavioralRandomFunctionModel("0.005");
            var coarseModel = CreateBehavioralRandomFunctionModel("0.01");

            AssertNoValidationIssues(fineModel.ValidationResult);
            AssertNoValidationIssues(coarseModel.ValidationResult);

            var fineRand = RunTransientSimulation(fineModel, "V(randout)");
            var fineRandom = RunTransientSimulation(fineModel, "V(randomout)");
            var fineWhite = RunTransientSimulation(fineModel, "V(whiteout)");
            var coarseRand = RunTransientSimulation(coarseModel, "V(randout)");
            var coarseRandom = RunTransientSimulation(coarseModel, "V(randomout)");
            var coarseWhite = RunTransientSimulation(coarseModel, "V(whiteout)");

            Assert.All(fineRand, point => Assert.InRange(point.Item2, 0.0, 1.0));
            Assert.All(fineRandom, point => Assert.InRange(point.Item2, 0.0, 1.0));
            Assert.All(fineWhite, point => Assert.InRange(point.Item2, -0.5, 0.5));

            Assert.Equal(Sample(fineRand, 0.05), Sample(fineRand, 0.20), 12);
            Assert.Equal(Sample(fineRand, 0.30), Sample(fineRand, 0.45), 12);

            foreach (var time in new[] { 0.05, 0.125, 0.20, 0.30, 0.375, 0.45, 0.55, 0.625, 0.70, 0.80, 0.875, 0.95 })
            {
                Assert.InRange(Math.Abs(Sample(fineRand, time) - Sample(coarseRand, time)), 0.0, 1e-9);
                Assert.InRange(Math.Abs(Sample(fineRandom, time) - Sample(coarseRandom, time)), 0.0, 3e-3);
                Assert.InRange(Math.Abs(Sample(fineWhite, time) - Sample(coarseWhite, time)), 0.0, 3e-3);
            }
        }

        [Theory]
        [InlineData("~0", 1.0)]
        [InlineData("~1", 0.0)]
        [InlineData("xor(0,0)", 0.0)]
        [InlineData("xor(0,1)", 1.0)]
        [InlineData("xor(1,0)", 1.0)]
        [InlineData("xor(1,1)", 0.0)]
        public void When_LtspiceBooleanAliasesAreRead_Expect_OpReferenceValue(string expression, double expected)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - boolean expression aliases",
                "B1 out 0 V={" + expression + "}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(expected, RunOpSimulation(model, "V(out)")));
        }

        [Theory]
        [InlineData("0^0", 0.0)]
        [InlineData("0^1", 1.0)]
        [InlineData("1^0", 1.0)]
        [InlineData("1^1", 0.0)]
        public void When_PspiceCaretXorIsReadInPspiceMode_Expect_OpReferenceValue(
            string expression,
            double expected)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.PSpice,
                "PSpice - caret XOR",
                "B1 out 0 V={" + expression + "}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(expected, RunOpSimulation(model, "V(out)")));
        }

        [Theory]
        [InlineData("~0", 1.0)]
        [InlineData("~1", 0.0)]
        public void When_PspiceTildeNotIsReadInPspiceMode_Expect_OpReferenceValue(
            string expression,
            double expected)
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.PSpice,
                "PSpice - tilde NOT",
                "B1 out 0 V={" + expression + "}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(expected, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_PspiceBooleanOperatorsAreUsedByFunc_Expect_ResolvedValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.PSpice,
                "PSpice - boolean operators in func",
                ".func invert(a)=~a",
                "B1 out 0 V={invert(0)}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_PspiceTildeNotIsUsedByParam_Expect_ResolvedValue()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.PSpice,
                "PSpice - tilde NOT in param",
                ".param notzero={~0}",
                "V1 out 0 {notzero}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_CaretPowerIsReadByDefault_Expect_Spice3ExponentBehavior()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.None,
                "SPICE3f5 - caret exponent",
                "B1 out 0 V={2^3}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(8.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_CaretPowerIsReadInLtspiceMode_Expect_ExistingExponentBehavior()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - caret exponent",
                "B1 out 0 V={2^3}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(8.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_LaplaceCaretPowerIsReadInLtspiceMode_Expect_ExistingExponentBehavior()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - Laplace caret exponent",
                "VIN in 0 2",
                "E1 out 0 LAPLACE {V(in)} = {1/(s^2+1)}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(2.0, RunOpSimulation(model, "V(out)")));
        }

        [Fact]
        public void When_LtspiceBooleanAliasesAreUsedByFunc_Expect_ResolvedValues()
        {
            var model = GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - boolean aliases in func",
                ".func exclusive(a,b)=xor(a,b)",
                "B1 out 0 V={~0+exclusive(1,0)}",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");

            AssertNoValidationIssues(model.ValidationResult);
            Assert.True(EqualsWithTol(2.0, RunOpSimulation(model, "V(out)")));
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

        private static double FiniteSineReference(double time)
        {
            const double offset = 1.0;
            const double amplitude = 2.0;
            const double frequency = 250e6;
            const double delay = 1e-9;
            const double theta = 0.0;
            const double phase = 0.0;
            const double cycleCount = 2.0;

            if (time <= delay || time >= delay + (cycleCount / frequency))
            {
                return offset;
            }

            var localTime = time - delay;
            var result = amplitude * Math.Sin((2.0 * Math.PI * frequency * localTime) + (phase * Math.PI / 180.0));
            if (!theta.Equals(0.0))
            {
                result *= Math.Exp(-localTime * theta);
            }

            return offset + result;
        }

        private static double PwlFileReference(double time)
        {
            if (time <= 1e-9)
            {
                return time / 1e-9;
            }

            return Math.Max(0.0, (2e-9 - time) / 1e-9);
        }

        private static double PwlScaledReference(double time)
        {
            if (time <= 2.0)
            {
                return 3.0 + (1.5 * time);
            }

            return Math.Max(0.0, 12.0 - (3.0 * time));
        }

        private static double PwlLastScaleFactorReference(double time)
        {
            if (time <= 4.0)
            {
                return -0.75 * time;
            }

            return Math.Min(0.0, (0.75 * time) - 6.0);
        }

        private static double PwlRepeatForReference(double time)
        {
            const double period = 3e-9;
            const double firstPointTime = 1e-9;
            const double stopTime = 9e-9;

            if (time >= stopTime)
            {
                return 3.0;
            }

            var cycle = (int)Math.Floor(time / period);
            var localTime = time - (cycle * period);
            if (localTime < firstPointTime)
            {
                if (cycle == 0)
                {
                    return 1.0;
                }

                return 3.0 + ((1.0 - 3.0) * localTime / firstPointTime);
            }

            return 1.0 + ((3.0 - 1.0) * (localTime - firstPointTime) / (period - firstPointTime));
        }

        private static double PwlRepeatTriangleReference(double time)
        {
            const double halfPeriod = 1e-9;
            const double period = 2e-9;
            var localTime = time % period;
            if (localTime <= halfPeriod)
            {
                return localTime / halfPeriod;
            }

            return (period - localTime) / halfPeriod;
        }

        private static double PwlFiniteRepeatTriangleReference(double time)
        {
            const double stopTime = 4e-9;
            if (time >= stopTime)
            {
                return 0.0;
            }

            return PwlRepeatTriangleReference(time);
        }

        private static double PwlScaledRepeatReference(double time)
        {
            if (time >= 8.0)
            {
                return 0.0;
            }

            var localTime = time % 4.0;
            if (localTime <= 2.0)
            {
                return 2.0 * localTime;
            }

            return 2.0 * (4.0 - localTime);
        }

        private static SpiceSharpModel CreateBehavioralRandomFunctionModel(string step)
        {
            return GetSpiceSharpModelWithCompatibility(
                CompatibilityOptions.LTspice,
                "LTspice P2 - behavioral random functions",
                "BRAND randout 0 V={rand(time*4)}",
                "BRANDOM randomout 0 V={random(time*4)}",
                "BWHITE whiteout 0 V={white(time*4)}",
                "RRAND randout 0 1k",
                "RRANDOM randomout 0 1k",
                "RWHITE whiteout 0 1k",
                ".tran " + step + " 1 0 " + step,
                ".save V(randout) V(randomout) V(whiteout)",
                ".end");
        }

        private static double Sample(IReadOnlyList<Tuple<double, double>> points, double time)
        {
            if (time <= points[0].Item1)
            {
                return points[0].Item2;
            }

            for (var index = 1; index < points.Count; index++)
            {
                if (time > points[index].Item1)
                {
                    continue;
                }

                var before = points[index - 1];
                var after = points[index];
                var width = after.Item1 - before.Item1;
                if (Math.Abs(width) < 1e-30)
                {
                    return after.Item2;
                }

                var fraction = (time - before.Item1) / width;
                return before.Item2 + ((after.Item2 - before.Item2) * fraction);
            }

            return points[points.Count - 1].Item2;
        }

        public static IEnumerable<object[]> PwlFileFormatCases()
        {
            yield return new object[]
            {
                Lines(
                    "0,0",
                    "1e-9,1",
                    "2e-9,0"),
            };

            yield return new object[]
            {
                Lines(
                    string.Empty,
                    "; generated PWL data",
                    "# time value data",
                    "* exported source waveform",
                    "0 0",
                    "1e-9 1",
                    "2e-9 0"),
            };

            yield return new object[]
            {
                Lines(
                    "time;value",
                    "0;0",
                    "1e-9;1",
                    "2e-9;0"),
            };

            yield return new object[]
            {
                Lines(
                    "time\tvalue",
                    "0\t0",
                    "1e-9\t1",
                    "2e-9\t0"),
            };
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private static void WriteMonoWaveFile(string path)
        {
            new WaveFileWriter().Write(
                path,
                1000,
                1.0,
                16,
                new[] { (0.0, 0.0), (0.001, 0.5), (0.002, 0.0) });
        }

            private static void WriteStereoWaveFile(string path)
            {
                new WaveFileWriter().Write(
                path,
                1000,
                1.0,
                16,
                new[] { (0.0, 0.0), (0.001, 0.5), (0.002, 0.0) },
                new[] { (0.0, 0.0), (0.001, -0.5), (0.002, 0.0) });
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
