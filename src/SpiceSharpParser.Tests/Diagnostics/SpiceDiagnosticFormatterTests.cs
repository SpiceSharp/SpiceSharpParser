using System;
using System.Linq;
using System.Text.Json;
using SpiceSharpParser.Diagnostics;
using Xunit;

namespace SpiceSharpParser.Tests.Diagnostics
{
    public class SpiceDiagnosticFormatterTests
    {
        [Fact]
        public void ToJson_WhenDiagnosticHasRichMetadata_PreservesRawPolicyAndLocationData()
        {
            SpiceDiagnostic diagnostic = RichDiagnostic(
                SpiceDiagnosticCodes.CompatibilityApproximation,
                DiagnosticSeverity.Warning);
            var policy = new SpiceDiagnosticPolicy { WarningsAsErrors = true };
            var result = new SpiceCompilationResult(
                null,
                null,
                null,
                new[] { diagnostic },
                SpiceDialect.LTspice,
                diagnosticPolicy: policy);

            string first = SpiceDiagnosticFormatter.ToJson(result);
            string second = SpiceDiagnosticFormatter.ToJson(result);

            Assert.Equal(first, second);
            using JsonDocument document = JsonDocument.Parse(first);
            JsonElement root = document.RootElement;
            Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
            Assert.False(root.GetProperty("success").GetBoolean());
            Assert.False(root.GetProperty("policySuccess").GetBoolean());
            Assert.Equal("Error", root.GetProperty("diagnostics")[0].GetProperty("severity").GetString());
            Assert.Equal("Warning", root.GetProperty("allDiagnostics")[0].GetProperty("severity").GetString());

            JsonElement raw = root.GetProperty("allDiagnostics")[0];
            Assert.Equal("ParserShim", raw.GetProperty("compatibilityClass").GetString());
            Assert.Equal("PWL", raw.GetProperty("construct").GetString());
            Assert.Equal(7, raw.GetProperty("span").GetProperty("start").GetProperty("line").GetInt32());
            Assert.Equal(2, raw.GetProperty("span").GetProperty("start").GetProperty("column").GetInt32());
            Assert.Equal(2, raw.GetProperty("includeStack").GetArrayLength());
            Assert.Equal(1, raw.GetProperty("relatedLocations").GetArrayLength());
            Assert.Contains("#ssp6003", raw.GetProperty("helpLink").GetString());
            Assert.Empty(root.GetProperty("suppressedDiagnostics").EnumerateArray());
        }

        [Fact]
        public void ToSarif_WhenDiagnosticsAreSuppressed_EmitsOnlyEffectiveResultsAndSortedRules()
        {
            SpiceDiagnostic warning = RichDiagnostic(
                SpiceDiagnosticCodes.IgnoredConstruct,
                DiagnosticSeverity.Warning);
            var error = new SpiceDiagnostic(
                SpiceDiagnosticCodes.ParserError,
                DiagnosticSeverity.Error,
                DiagnosticStage.Parser,
                "Unexpected token.",
                new SourceSpan(
                    @"C:\Models\root model.net",
                    new SourcePosition(3, 4),
                    new SourcePosition(3, 5)));
            var policy = new SpiceDiagnosticPolicy();
            policy.SuppressedCodes.Add(SpiceDiagnosticCodes.IgnoredConstruct);
            var result = new SpiceCompilationResult(
                null,
                null,
                null,
                new[] { warning, error },
                SpiceDialect.Spice3,
                diagnosticPolicy: policy);

            string first = SpiceDiagnosticFormatter.ToSarif(result);
            string second = SpiceDiagnosticFormatter.ToSarif(result);

            Assert.Equal(first, second);
            using JsonDocument document = JsonDocument.Parse(first);
            JsonElement root = document.RootElement;
            Assert.Equal("2.1.0", root.GetProperty("version").GetString());
            JsonElement run = root.GetProperty("runs")[0];
            JsonElement rules = run.GetProperty("tool").GetProperty("driver").GetProperty("rules");
            JsonElement rule = Assert.Single(rules.EnumerateArray());
            Assert.Equal(SpiceDiagnosticCodes.ParserError, rule.GetProperty("id").GetString());
            Assert.Contains("#ssp1100", rule.GetProperty("helpUri").GetString());

            JsonElement sarifResult = Assert.Single(run.GetProperty("results").EnumerateArray());
            Assert.Equal(SpiceDiagnosticCodes.ParserError, sarifResult.GetProperty("ruleId").GetString());
            Assert.Equal("error", sarifResult.GetProperty("level").GetString());
            JsonElement physicalLocation = sarifResult
                .GetProperty("locations")[0]
                .GetProperty("physicalLocation");
            Assert.Equal(
                "file:///C:/Models/root%20model.net",
                physicalLocation.GetProperty("artifactLocation").GetProperty("uri").GetString());
            Assert.Equal(3, physicalLocation.GetProperty("region").GetProperty("startLine").GetInt32());
            Assert.Equal(1, run.GetProperty("properties").GetProperty("suppressedDiagnosticCount").GetInt32());
        }

        [Fact]
        public void ToSarif_WhenSeveralRulesAreVisible_SortsRuleMetadataAndKeepsResultOrder()
        {
            SpiceDiagnostic compatibility = RichDiagnostic(
                SpiceDiagnosticCodes.CompatibilityApproximation,
                DiagnosticSeverity.Warning);
            var syntax = new SpiceDiagnostic(
                SpiceDiagnosticCodes.ParserError,
                DiagnosticSeverity.Error,
                DiagnosticStage.Parser,
                "Unexpected token.");
            var result = new SpiceCompilationResult(
                null,
                null,
                null,
                new[] { compatibility, syntax },
                SpiceDialect.LTspice);

            using JsonDocument document = JsonDocument.Parse(
                SpiceDiagnosticFormatter.ToSarif(result));
            JsonElement run = document.RootElement.GetProperty("runs")[0];
            string[] ruleIds = run
                .GetProperty("tool")
                .GetProperty("driver")
                .GetProperty("rules")
                .EnumerateArray()
                .Select(rule => rule.GetProperty("id").GetString())
                .ToArray();
            string[] resultIds = run
                .GetProperty("results")
                .EnumerateArray()
                .Select(item => item.GetProperty("ruleId").GetString())
                .ToArray();

            Assert.Equal(
                new[] { SpiceDiagnosticCodes.ParserError, SpiceDiagnosticCodes.CompatibilityApproximation },
                ruleIds);
            Assert.Equal(
                new[] { SpiceDiagnosticCodes.CompatibilityApproximation, SpiceDiagnosticCodes.ParserError },
                resultIds);
            JsonElement properties = run.GetProperty("results")[0].GetProperty("properties");
            Assert.Equal("Review the lowered behavior.", properties.GetProperty("suggestedFix").GetString());
            Assert.Equal(2, properties.GetProperty("includeStack").GetArrayLength());
        }

        [Fact]
        public void BuiltInDiagnosticCodes_HaveStableHelpLinks()
        {
            Assert.Equal(
                SpiceDiagnosticCodes.All.Count,
                SpiceDiagnosticCodes.All.Distinct(StringComparer.OrdinalIgnoreCase).Count());

            foreach (string code in SpiceDiagnosticCodes.All)
            {
                Uri helpLink = SpiceDiagnosticCodes.GetHelpLink(code);
                Assert.NotNull(helpLink);
                Assert.EndsWith("#" + code.ToLowerInvariant(), helpLink.AbsoluteUri);

                var diagnostic = new SpiceDiagnostic(
                    code,
                    DiagnosticSeverity.Info,
                    DiagnosticStage.Linter,
                    "Test.");
                Assert.Equal(helpLink, diagnostic.HelpLink);
            }

            Assert.Null(SpiceDiagnosticCodes.GetHelpLink("CUSTOM001"));
        }

        private static SpiceDiagnostic RichDiagnostic(
            string code,
            DiagnosticSeverity severity)
        {
            var rootInclude = new SourceSpan(
                @"C:\Models\root.net",
                new SourcePosition(2, 1),
                new SourcePosition(2, 27));
            var nestedInclude = new SourceSpan(
                @"C:\Models\parent.inc",
                new SourcePosition(4, 1),
                new SourcePosition(4, 25));
            var related = new DiagnosticRelatedLocation(
                rootInclude,
                "Included from the root.");

            return new SpiceDiagnostic(
                code,
                severity,
                DiagnosticStage.Reader,
                "Construct \"PWL\" was lowered.\nReview its timing.",
                new SourceSpan(
                    @"C:\Models\child.inc",
                    new SourcePosition(7, 2),
                    new SourcePosition(7, 11)),
                new[] { related },
                "PWL",
                "Review the lowered behavior.",
                CompatibilityClass.ParserShim,
                includeStack: new[] { rootInclude, nestedInclude });
        }
    }
}
