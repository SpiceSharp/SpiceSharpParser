using System;
using System.Linq;
using SpiceSharpParser.Diagnostics;
using Xunit;

namespace SpiceSharpParser.Tests.Compilation
{
    public class SpiceDiagnosticPolicyTests
    {
        [Fact]
        public void CompileText_WhenWarningsAreErrors_FailsPolicyButKeepsSimulationReadyModel()
        {
            var options = new SpiceCompileOptions();
            options.DiagnosticPolicy.WarningsAsErrors = true;

            SpiceCompilationResult result = SpiceCompiler.CompileText(
                NetlistWithoutSimulation(),
                "warning.net",
                options);

            SpiceDiagnostic raw = Assert.Single(
                result.AllDiagnostics,
                diagnostic => diagnostic.Code == SpiceDiagnosticCodes.NoSimulation);
            SpiceDiagnostic effective = Assert.Single(
                result.Diagnostics,
                diagnostic => diagnostic.Code == SpiceDiagnosticCodes.NoSimulation);
            Assert.Equal(DiagnosticSeverity.Warning, raw.Severity);
            Assert.Equal(DiagnosticSeverity.Error, effective.Severity);
            Assert.True(result.Success);
            Assert.NotNull(result.Model);
            Assert.False(result.PolicySuccess);
            Assert.Equal(0, result.Compatibility.BlockerCount);
        }

        [Fact]
        public void CompileText_WhenWarningIsSuppressed_SeparatesRawAndEffectiveDiagnostics()
        {
            var options = new SpiceCompileOptions();
            options.DiagnosticPolicy.SuppressedCodes.Add(
                SpiceDiagnosticCodes.NoSimulation.ToLowerInvariant());

            SpiceCompilationResult result = SpiceCompiler.CompileText(
                NetlistWithoutSimulation(),
                "suppressed.net",
                options);

            Assert.Empty(result.Diagnostics);
            SpiceDiagnostic raw = Assert.Single(result.AllDiagnostics);
            SpiceDiagnostic suppressed = Assert.Single(result.SuppressedDiagnostics);
            Assert.Same(raw, suppressed);
            Assert.Equal(SpiceDiagnosticCodes.NoSimulation, suppressed.Code);
            Assert.True(result.Success);
            Assert.True(result.PolicySuccess);
            Assert.Equal(1, result.Compatibility.IssueCount);
        }

        [Fact]
        public void CompileText_WhenInfoIsPromotedToError_FailsOnlyPolicy()
        {
            var options = new SpiceCompileOptions();
            options.DiagnosticPolicy.SeverityOverrides[SpiceDiagnosticCodes.NoExports] =
                DiagnosticSeverity.Error;

            SpiceCompilationResult result = SpiceCompiler.CompileText(
                NetlistWithoutExports(),
                "promoted.net",
                options);

            SpiceDiagnostic raw = Assert.Single(result.AllDiagnostics);
            SpiceDiagnostic effective = Assert.Single(result.Diagnostics);
            Assert.Equal(SpiceDiagnosticCodes.NoExports, raw.Code);
            Assert.Equal(DiagnosticSeverity.Info, raw.Severity);
            Assert.Equal(DiagnosticSeverity.Error, effective.Severity);
            Assert.True(result.Success);
            Assert.False(result.PolicySuccess);
        }

        [Fact]
        public void CompileText_WhenWarningIsOverriddenToInfo_WarningsAsErrorsDoesNotPromoteIt()
        {
            var options = new SpiceCompileOptions();
            options.DiagnosticPolicy.WarningsAsErrors = true;
            options.DiagnosticPolicy.SeverityOverrides[SpiceDiagnosticCodes.NoSimulation] =
                DiagnosticSeverity.Info;

            SpiceCompilationResult result = SpiceCompiler.CompileText(
                NetlistWithoutSimulation(),
                "override.net",
                options);

            SpiceDiagnostic effective = Assert.Single(result.Diagnostics);
            Assert.Equal(DiagnosticSeverity.Info, effective.Severity);
            Assert.True(result.Success);
            Assert.True(result.PolicySuccess);
        }

        [Fact]
        public void CompileText_WhenErrorIsSuppressedAndDowngraded_KeepsItVisibleAndBlocking()
        {
            var options = new SpiceCompileOptions { RunLinter = false };
            options.DiagnosticPolicy.SuppressedCodes.Add(SpiceDiagnosticCodes.ParserError);
            options.DiagnosticPolicy.SeverityOverrides[SpiceDiagnosticCodes.ParserError] =
                DiagnosticSeverity.Info;

            SpiceCompilationResult result = SpiceCompiler.CompileText(
                Lines(
                    "unsafe policy",
                    "R1 out 0 1k"),
                "unsafe.net",
                options);

            SpiceDiagnostic raw = Assert.Single(result.AllDiagnostics);
            SpiceDiagnostic effective = Assert.Single(result.Diagnostics);
            Assert.Equal(SpiceDiagnosticCodes.ParserError, effective.Code);
            Assert.Equal(DiagnosticSeverity.Error, raw.Severity);
            Assert.Equal(DiagnosticSeverity.Error, effective.Severity);
            Assert.Empty(result.SuppressedDiagnostics);
            Assert.False(result.Success);
            Assert.False(result.PolicySuccess);
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenCompatibilityWarningIsSuppressed_PreservesRawCompatibilityReport()
        {
            var options = new SpiceCompileOptions
            {
                Dialect = SpiceDialect.LTspice,
                RunLinter = false,
            };
            options.DiagnosticPolicy.SuppressedCodes.Add(SpiceDiagnosticCodes.IgnoredConstruct);

            SpiceCompilationResult result = SpiceCompiler.CompileText(
                Lines(
                    "suppressed compatibility",
                    "V1 out 0 1",
                    "R1 out 0 1k",
                    ".backanno",
                    ".op",
                    ".save V(out)",
                    ".end"),
                "compatibility.net",
                options);

            Assert.Empty(result.Diagnostics);
            Assert.Single(result.SuppressedDiagnostics);
            Assert.Single(result.AllDiagnostics);
            Assert.Single(result.Compatibility.Ignored);
            Assert.Equal(1, result.Compatibility.WarningCount);
            Assert.True(result.Success);
            Assert.True(result.PolicySuccess);
        }

        [Fact]
        public void CompileText_WhenDiagnosticPolicyIsInvalid_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => SpiceCompiler.CompileText(
                NetlistWithoutSimulation(),
                new SpiceCompileOptions { DiagnosticPolicy = null }));

            var options = new SpiceCompileOptions();
            options.DiagnosticPolicy.SuppressedCodes.Add(" ");
            Assert.Throws<ArgumentException>(() => SpiceCompiler.CompileText(
                NetlistWithoutSimulation(),
                options));
        }

        private static string NetlistWithoutSimulation()
        {
            return Lines(
                "policy warning",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".end");
        }

        private static string NetlistWithoutExports()
        {
            return Lines(
                "policy info",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".op",
                ".end");
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
    }
}
