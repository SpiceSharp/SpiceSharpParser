using System;
using System.Linq;
using SpiceSharpParser.Diagnostics;
using Xunit;

namespace SpiceSharpParser.Tests.Compilation
{
    public class CompatibilityReportTests
    {
        [Fact]
        public void CompileText_WhenBackannoUsesDifferentDialects_ReportsDialectSpecificCompatibility()
        {
            string source = Lines(
                "compatibility dialect report",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".backanno",
                ".op",
                ".save V(out)",
                ".end");

            SpiceCompilationResult spice3 = SpiceCompiler.CompileText(
                source,
                "dialect.net",
                new SpiceCompileOptions { RunLinter = false });
            SpiceCompilationResult ltspice = SpiceCompiler.CompileText(
                source,
                "dialect.net",
                new SpiceCompileOptions
                {
                    Dialect = SpiceDialect.LTspice,
                    RunLinter = false,
                });

            SpiceDiagnostic unsupported = Assert.Single(spice3.Compatibility.Unsupported);
            Assert.Equal(SpiceDiagnosticCodes.UnsupportedControl, unsupported.Code);
            Assert.Equal(CompatibilityClass.TargetedDiagnostic, unsupported.CompatibilityClass);
            Assert.Equal(".backanno", unsupported.Construct);
            Assert.Equal(1, spice3.Compatibility.BlockerCount);
            Assert.False(spice3.Compatibility.CanSimulate);
            Assert.False(spice3.Compatibility.IsFullyCompatible);
            Assert.Equal(1, spice3.Compatibility.IssuesByConstruct[".backanno"]);

            SpiceDiagnostic ignored = Assert.Single(ltspice.Compatibility.Ignored);
            Assert.Equal(SpiceDiagnosticCodes.IgnoredConstruct, ignored.Code);
            Assert.Equal(CompatibilityClass.RecognizedNoOp, ignored.CompatibilityClass);
            Assert.Empty(ltspice.Compatibility.Unsupported);
            Assert.Equal(0, ltspice.Compatibility.BlockerCount);
            Assert.Equal(1, ltspice.Compatibility.WarningCount);
            Assert.True(ltspice.Compatibility.CanSimulate);
            Assert.True(ltspice.Compatibility.IsFullyCompatible);
        }

        [Fact]
        public void CompileText_WhenModelRequiresEngineSupport_ReportsUnsupportedConstruct()
        {
            string source = Lines(
                "compatibility engine report",
                ".model pwr VDMOS(Ron=1 Vto=2)",
                ".op",
                ".end");
            var options = new SpiceCompileOptions
            {
                Dialect = SpiceDialect.LTspice,
                RunLinter = false,
            };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "vdmos.net", options);

            SpiceDiagnostic issue = Assert.Single(result.Compatibility.Unsupported);
            Assert.Equal(SpiceDiagnosticCodes.UnsupportedModel, issue.Code);
            Assert.Equal(CompatibilityClass.EngineRequired, issue.CompatibilityClass);
            Assert.Equal("VDMOS", issue.Construct);
            Assert.Equal(1, result.Compatibility.BlockerCount);
            Assert.Equal(1, result.Compatibility.IssuesByConstruct["VDMOS"]);
            Assert.Equal(1, result.Compatibility.IssuesByFile["vdmos.net"]);
            Assert.Equal(1, result.Compatibility.IssuesByCode[SpiceDiagnosticCodes.UnsupportedModel]);
            Assert.False(result.Compatibility.CanSimulate);
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenSeveralStagesReportIssues_AggregatesEveryDiagnosticDeterministically()
        {
            string source = Lines(
                "compatibility aggregation",
                "=",
                "C1 floating 0 1u",
                ".backanno",
                ".op",
                ".end");

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "aggregate.net");

            Assert.Equal(result.Diagnostics.Count, result.Compatibility.IssueCount);
            Assert.Equal(
                result.Diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error),
                result.Compatibility.BlockerCount);
            Assert.Contains(result.Compatibility.Unclassified, diagnostic => diagnostic.Stage == DiagnosticStage.Parser);
            Assert.Contains(result.Compatibility.Unsupported, diagnostic => diagnostic.Code == SpiceDiagnosticCodes.UnsupportedControl);
            Assert.Equal(
                result.Compatibility.IssuesByCode.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase),
                result.Compatibility.IssuesByCode.Keys);
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }
    }
}
