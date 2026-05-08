using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.LTspiceCompatibility
{
    public class LTspiceCompatibilityP0Tests : BaseTests
    {
        public static IEnumerable<object[]> RunnableOpFixtures
        {
            get
            {
                yield return new object[]
                {
                    "B source",
                    new[]
                    {
                        "LTspice P0 - B source",
                        "VIN in 0 2",
                        "B1 out 0 V={V(in)*0.5}",
                        "RLOAD out 0 1k",
                        ".OP",
                        ".SAVE V(out)",
                        ".END",
                    },
                    "V(out)",
                    1.0,
                };

                yield return new object[]
                {
                    "VALUE controlled source",
                    new[]
                    {
                        "LTspice P0 - VALUE source",
                        "VIN in 0 2",
                        "E1 out 0 VALUE={V(in)+1}",
                        "RLOAD out 0 1k",
                        ".OP",
                        ".SAVE V(out)",
                        ".END",
                    },
                    "V(out)",
                    3.0,
                };

                yield return new object[]
                {
                    "source-level TABLE",
                    new[]
                    {
                        "LTspice P0 - TABLE source",
                        "V1 1 0 1.5m",
                        "R1 1 0 10",
                        "E12 2 1 TABLE {V(1,0)} = (0,1) (1m,2) (2m,3)",
                        "R2 2 0 10",
                        ".OP",
                        ".SAVE V(2,1)",
                        ".END",
                    },
                    "V(2,1)",
                    2.5,
                };

                yield return new object[]
                {
                    "POLY source",
                    new[]
                    {
                        "LTspice P0 - POLY source",
                        "R1 1 0 100",
                        "V1 1 0 2",
                        "ESource 2 0 POLY(1) 1 0 2 1",
                        ".OP",
                        ".SAVE V(2,0)",
                        ".END",
                    },
                    "V(2,0)",
                    4.0,
                };

                yield return new object[]
                {
                    "source-level LAPLACE",
                    new[]
                    {
                        "LTspice P0 - source-level LAPLACE",
                        ".PARAM tau=1u",
                        "VIN in 0 1",
                        "ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)}",
                        "RLOAD out 0 1k",
                        ".OP",
                        ".SAVE V(out)",
                        ".END",
                    },
                    "V(out)",
                    1.0,
                };

                yield return new object[]
                {
                    "function-style LAPLACE",
                    new[]
                    {
                        "LTspice P0 - function-style LAPLACE",
                        ".PARAM tau=1u",
                        "VIN in 0 1",
                        "E1 out 0 VALUE={LAPLACE(V(in), 1/(1+s*tau))}",
                        "RLOAD out 0 1k",
                        ".OP",
                        ".SAVE V(out)",
                        ".END",
                    },
                    "V(out)",
                    1.0,
                };

                yield return new object[]
                {
                    ".param and .func",
                    new[]
                    {
                        "LTspice P0 - params and funcs",
                        ".PARAM gain=2",
                        ".FUNC scale(x) {x*gain}",
                        "VIN in 0 2",
                        "B1 out 0 V={scale(V(in))}",
                        "RLOAD out 0 1k",
                        ".OP",
                        ".SAVE V(out)",
                        ".END",
                    },
                    "V(out)",
                    4.0,
                };
            }
        }

        public static IEnumerable<object[]> KnownUnsupportedLtspiceControls
        {
            get
            {
                yield return new object[] { ".backanno", ".backanno" };
                yield return new object[] { ".tf", ".tf V(out) VIN" };
                yield return new object[] { ".four", ".four 1k V(out)" };
                yield return new object[] { ".net", ".net V(out) VIN" };
                yield return new object[] { ".ferret", ".ferret https://example.invalid/vendor.lib" };
                yield return new object[] { ".loadbias", ".loadbias bias.raw" };
                yield return new object[] { ".savebias", ".savebias bias.raw" };
                yield return new object[] { ".machine", ".machine state_model" };
                yield return new object[] { ".endmachine", ".endmachine" };
            }
        }

        public static IEnumerable<object[]> UnsupportedSyntaxAuditFixtures
        {
            get
            {
                yield return new object[]
                {
                    ".tran one-argument form",
                    ".tran",
                    new[]
                    {
                        "LTspice P0 - one-argument tran",
                        "V1 out 0 1",
                        "R1 out 0 1k",
                        ".tran 1m",
                        ".save V(out)",
                        ".end",
                    },
                };

                yield return new object[]
                {
                    ".options plotwinsize",
                    "plotwinsize",
                    new[]
                    {
                        "LTspice P0 - plotwinsize option",
                        "V1 out 0 1",
                        "R1 out 0 1k",
                        ".op",
                        ".options plotwinsize=0",
                        ".save V(out)",
                        ".end",
                    },
                };

                yield return new object[]
                {
                    "PULSE cycle count",
                    "PULSE",
                    new[]
                    {
                        "LTspice P0 - PULSE cycle count",
                        "V1 out 0 PULSE(0 1 0 1n 1n 10n 20n 3)",
                        "R1 out 0 1k",
                        ".tran 1n 50n",
                        ".save V(out)",
                        ".end",
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RunnableOpFixtures))]
        public void When_RunnableBaselineFixtureIsRead_Expect_NoValidationErrorsAndReference(string name, string[] lines, string exportName, double expected)
        {
            var model = GetSpiceSharpModel(lines);

            AssertNoValidationIssues(name, model.ValidationResult);
            var actual = RunOpSimulation(model, exportName);
            Assert.True(EqualsWithTol(expected, actual), $"{name}: expected {expected}, got {actual}.");
        }

        [Fact]
        public void When_TransientBaselineUsesSaveAndMeasure_Expect_Reference()
        {
            var model = GetSpiceSharpModel(
                "LTspice P0 - transient save and measure",
                "VIN in 0 PULSE(0 1 0 1n 1n 10n 20n)",
                "R1 in out 1k",
                "C1 out 0 1n",
                ".tran 1n 50n",
                ".save V(out)",
                ".meas tran vmax MAX V(out)",
                ".end");

            AssertNoValidationIssues("transient save and measure", model.ValidationResult);
            var exports = RunTransientSimulation(model, "V(out)");

            Assert.NotEmpty(exports);
            AssertMeasurementSuccess(model, "vmax");
        }

        [Theory]
        [InlineData("quoted include")]
        [InlineData("relative include")]
        public void When_IncludeFixtureIsRead_Expect_IncludedElement(string includeStyle)
        {
            string tempDirectory = CreateTempDirectory();

            try
            {
                string includePath;
                string includeStatement;

                if (includeStyle == "relative include")
                {
                    string modelDirectory = Path.Combine(tempDirectory, "models");
                    Directory.CreateDirectory(modelDirectory);
                    includePath = Path.Combine(modelDirectory, "load.inc");
                    includeStatement = ".include " + Path.Combine("models", "load.inc");
                }
                else
                {
                    includePath = Path.Combine(tempDirectory, "load.inc");
                    includeStatement = $".include \"{includePath}\"";
                }

                File.WriteAllText(includePath, "RLOAD out 0 1k" + Environment.NewLine);

                var model = GetSpiceSharpModelWithWorkingDirectoryParameter(
                    tempDirectory,
                    "LTspice P0 - include fixture",
                    "VIN out 0 1",
                    includeStatement,
                    ".op",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(includeStyle, model.ValidationResult);
                Assert.NotNull(model.Circuit["RLOAD"]);
                Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Theory]
        [InlineData("one-argument lib", ".lib loads.lib", "RLOAD", false)]
        [InlineData("selected-section lib", ".lib loads.lib slow", "RSLOW", true)]
        public void When_LibFixtureIsRead_Expect_SelectedContent(string name, string libStatement, string expectedEntity, bool useSections)
        {
            string tempDirectory = CreateTempDirectory();

            try
            {
                string libPath = Path.Combine(tempDirectory, "loads.lib");
                string libContent = useSections
                    ? ".lib fast" + Environment.NewLine
                      + "RFAST out 0 1k" + Environment.NewLine
                      + ".endl" + Environment.NewLine
                      + ".lib slow" + Environment.NewLine
                      + "RSLOW out 0 2k" + Environment.NewLine
                      + ".endl" + Environment.NewLine
                    : "RLOAD out 0 1k" + Environment.NewLine;
                File.WriteAllText(libPath, libContent);

                var model = GetSpiceSharpModelWithWorkingDirectoryParameter(
                    tempDirectory,
                    "LTspice P0 - lib fixture",
                    "VIN out 0 1",
                    libStatement,
                    ".op",
                    ".save V(out)",
                    ".end");

                AssertNoValidationIssues(name, model.ValidationResult);
                Assert.NotNull(model.Circuit[expectedEntity]);
                Assert.True(EqualsWithTol(1.0, RunOpSimulation(model, "V(out)")));
            }
            finally
            {
                DeleteDirectory(tempDirectory);
            }
        }

        [Theory]
        [MemberData(nameof(KnownUnsupportedLtspiceControls))]
        public void When_KnownUnsupportedLtspiceControlIsRead_Expect_TargetedDiagnostic(string directive, string directiveLine)
        {
            var model = GetSpiceSharpModel(
                "LTspice P0 - unsupported control",
                "VIN out 0 1",
                "R1 out 0 1k",
                directiveLine,
                ".op",
                ".save V(out)",
                ".end");

            Assert.True(model.ValidationResult.HasError);
            string messages = string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message));
            Assert.Contains("LTspice", messages);
            Assert.Contains(directive, messages, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [MemberData(nameof(UnsupportedSyntaxAuditFixtures))]
        public void When_UnsupportedSyntaxAuditFixtureIsRead_Expect_ValidationError(string name, string expectedText, string[] lines)
        {
            var model = GetSpiceSharpModel(lines);

            Assert.True(model.ValidationResult.HasError, $"{name}: expected a validation error.");
            string messages = string.Join(Environment.NewLine, model.ValidationResult.Errors.Select(error => error.Message));
            Assert.Contains(expectedText, messages, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void When_TranModifierFixtureIsParsed_Expect_ParseOnlyEvidence()
        {
            var parseResult = ParseNetlistRaw(
                false,
                "LTspice P0 - parse-only tran modifier",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".tran 1m startup",
                ".save V(out)",
                ".end");

            Assert.False(parseResult.ValidationResult.HasError);
            Assert.Contains(parseResult.FinalModel.Statements, statement => statement is Control control && control.Name.Equals("TRAN", StringComparison.OrdinalIgnoreCase));
        }

        private static string CreateTempDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), "SpiceSharpParserLtspiceP0_" + Guid.NewGuid().ToString("N"));
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

        private static void AssertNoValidationIssues(string name, ValidationEntryCollection validation)
        {
            string messages = string.Join(Environment.NewLine, validation.Select(entry => entry.Message));
            Assert.False(validation.HasError, $"{name}: unexpected validation error: {messages}");
            Assert.False(validation.HasWarning, $"{name}: unexpected validation warning: {messages}");
        }
    }
}
