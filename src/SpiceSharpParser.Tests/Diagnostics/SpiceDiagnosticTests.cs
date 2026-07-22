using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Validation;
using Xunit;

namespace SpiceSharpParser.Tests.Diagnostics
{
    public class SpiceDiagnosticTests
    {
        [Fact]
        public void When_LineInfoIsKnown_Expect_OneBasedEndExclusiveSourceSpan()
        {
            var lineInfo = new SpiceLineInfo
            {
                FileName = @"C:\Models\device.lib",
                LineNumber = 7,
                StartColumnIndex = 2,
                EndColumnIndex = 5,
            };

            var span = SourceSpan.FromLineInfo(lineInfo);

            Assert.True(span.IsKnown);
            Assert.Equal(lineInfo.FileName, span.FilePath);
            Assert.Equal(new SourcePosition(7, 3), span.Start);
            Assert.Equal(new SourcePosition(7, 6), span.End);
        }

        [Fact]
        public void When_LineInfoIsMissing_Expect_UnknownSourceSpan()
        {
            Assert.Equal(SourceSpan.Unknown, SourceSpan.FromLineInfo(null));
        }

        [Fact]
        public void When_OnlyFileNameIsKnown_Expect_FileNameWithUnknownPosition()
        {
            var lineInfo = new SpiceLineInfo { FileName = "input.cir" };

            SourceSpan span = SourceSpan.FromLineInfo(lineInfo);

            Assert.Equal("input.cir", span.FilePath);
            Assert.False(span.IsKnown);
        }

        [Fact]
        public void When_EndPrecedesStart_Expect_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SourceSpan(
                "test.cir",
                new SourcePosition(4, 8),
                new SourcePosition(4, 3)));
        }

        [Fact]
        public void When_RelatedLocationInputChanges_Expect_DiagnosticSnapshotIsUnchanged()
        {
            var relatedLocations = new List<DiagnosticRelatedLocation>
            {
                new DiagnosticRelatedLocation(SourceSpan.Unknown, "Included from here."),
            };

            var diagnostic = new SpiceDiagnostic(
                SpiceDiagnosticCodes.ReaderError,
                DiagnosticSeverity.Error,
                DiagnosticStage.Reader,
                "Unsupported component.",
                relatedLocations: relatedLocations);

            relatedLocations.Clear();

            Assert.Single(diagnostic.RelatedLocations);
        }

        [Fact]
        public void When_IncludeStackInputChanges_Expect_DiagnosticSnapshotIsUnchanged()
        {
            var includeStack = new List<SourceSpan>
            {
                new SourceSpan("root.cir", new SourcePosition(2, 1), new SourcePosition(2, 20)),
            };
            var diagnostic = new SpiceDiagnostic(
                SpiceDiagnosticCodes.ReaderError,
                DiagnosticSeverity.Error,
                DiagnosticStage.Reader,
                "Unsupported component.",
                includeStack: includeStack);

            includeStack.Clear();

            Assert.Single(diagnostic.IncludeStack);
        }

        [Theory]
        [InlineData(ValidationEntrySource.Lexer, SpiceDiagnosticCodes.LexerError, DiagnosticStage.Lexer)]
        [InlineData(ValidationEntrySource.Parser, SpiceDiagnosticCodes.ParserError, DiagnosticStage.Parser)]
        [InlineData(ValidationEntrySource.Processor, SpiceDiagnosticCodes.PreprocessorError, DiagnosticStage.Preprocessor)]
        [InlineData(ValidationEntrySource.Reader, SpiceDiagnosticCodes.ReaderError, DiagnosticStage.Reader)]
        public void When_ValidationEntryIsAdapted_Expect_StableCodeAndStage(
            ValidationEntrySource source,
            string expectedCode,
            DiagnosticStage expectedStage)
        {
            var lineInfo = new SpiceLineInfo
            {
                FileName = "input.cir",
                LineNumber = 3,
                StartColumnIndex = 4,
                EndColumnIndex = 9,
            };
            var entry = new ValidationEntry(
                source,
                ValidationEntryLevel.Warning,
                "Test diagnostic.",
                lineInfo);

            var diagnostic = entry.ToDiagnostic();

            Assert.Equal(expectedCode, diagnostic.Code);
            Assert.Equal(expectedStage, diagnostic.Stage);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            Assert.Equal("Test diagnostic.", diagnostic.Message);
            Assert.Equal(new SourcePosition(3, 5), diagnostic.Span.Start);
        }

        [Fact]
        public void When_ValidationCollectionIsAdapted_Expect_OrderIsPreserved()
        {
            var entries = new ValidationEntryCollection();
            entries.AddWarning(ValidationEntrySource.Processor, "First");
            entries.AddError(ValidationEntrySource.Reader, "Second");

            var diagnostics = entries.ToDiagnostics();

            Assert.Collection(
                diagnostics,
                first => Assert.Equal("First", first.Message),
                second => Assert.Equal("Second", second.Message));
        }

        [Theory]
        [InlineData(
            "Unsupported LTspice component 'O1': lossy transmission lines require SpiceSharp engine support.",
            SpiceDiagnosticCodes.UnsupportedComponent,
            CompatibilityClass.EngineRequired,
            "O1")]
        [InlineData(
            "Unsupported LTspice diode model parameter 'Ron': ideal-diode behavior is not mapped yet.",
            SpiceDiagnosticCodes.UnsupportedParameter,
            CompatibilityClass.TargetedDiagnostic,
            "Ron")]
        [InlineData(
            "Unsupported LTspice model type 'VDMOS': power MOSFET behavior requires SpiceSharp engine support.",
            SpiceDiagnosticCodes.UnsupportedModel,
            CompatibilityClass.EngineRequired,
            "VDMOS")]
        [InlineData(
            "Unsupported control: TF",
            SpiceDiagnosticCodes.UnsupportedControl,
            CompatibilityClass.TargetedDiagnostic,
            "TF")]
        [InlineData(
            "Unsupported waveform 'CUSTOM'.",
            SpiceDiagnosticCodes.UnsupportedWaveform,
            CompatibilityClass.TargetedDiagnostic,
            "CUSTOM")]
        [InlineData(
            "Unsupported option: plotwinsize",
            SpiceDiagnosticCodes.UnsupportedOption,
            CompatibilityClass.TargetedDiagnostic,
            "plotwinsize")]
        [InlineData(
            "Unsupported export: V(out)",
            SpiceDiagnosticCodes.UnsupportedExport,
            CompatibilityClass.TargetedDiagnostic,
            "V(out)")]
        [InlineData(
            "Ignored LTspice control '.backanno': generated metadata is not used.",
            SpiceDiagnosticCodes.IgnoredConstruct,
            CompatibilityClass.RecognizedNoOp,
            ".backanno")]
        public void When_ReaderCompatibilityEntryIsAdapted_ExpectSpecificMetadata(
            string message,
            string expectedCode,
            CompatibilityClass expectedClass,
            string expectedConstruct)
        {
            var entry = new ValidationEntry(
                ValidationEntrySource.Reader,
                ValidationEntryLevel.Error,
                message);

            SpiceDiagnostic diagnostic = entry.ToDiagnostic();

            Assert.Equal(expectedCode, diagnostic.Code);
            Assert.Equal(expectedClass, diagnostic.CompatibilityClass);
            Assert.Equal(expectedConstruct, diagnostic.Construct);
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.SuggestedFix));
        }

        [Fact]
        public void When_LintIssueIsAdapted_Expect_CategorySeverityAndFixArePreserved()
        {
            var issue = new LintIssue(
                LintSeverity.Info,
                LintCategory.NoExports,
                "No exports were requested.",
                "V(out)",
                "Add .SAVE V(out).");

            var diagnostic = issue.ToDiagnostic();

            Assert.Equal(SpiceDiagnosticCodes.NoExports, diagnostic.Code);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            Assert.Equal(DiagnosticStage.Linter, diagnostic.Stage);
            Assert.Equal("V(out)", diagnostic.Construct);
            Assert.Equal("Add .SAVE V(out).", diagnostic.SuggestedFix);
            Assert.Equal(SourceSpan.Unknown, diagnostic.Span);
        }

        [Theory]
        [InlineData(LintCategory.FloatingNode, SpiceDiagnosticCodes.FloatingNode)]
        [InlineData(LintCategory.MissingDCPath, SpiceDiagnosticCodes.MissingDcPath)]
        [InlineData(LintCategory.MissingModel, SpiceDiagnosticCodes.MissingModel)]
        [InlineData(LintCategory.DuplicateComponent, SpiceDiagnosticCodes.DuplicateComponent)]
        [InlineData(LintCategory.MissingACMagnitude, SpiceDiagnosticCodes.MissingAcMagnitude)]
        [InlineData(LintCategory.MissingTranMaxStep, SpiceDiagnosticCodes.MissingTranMaxStep)]
        [InlineData(LintCategory.EmptyCircuit, SpiceDiagnosticCodes.EmptyCircuit)]
        [InlineData(LintCategory.NoSimulation, SpiceDiagnosticCodes.NoSimulation)]
        [InlineData(LintCategory.NoExports, SpiceDiagnosticCodes.NoExports)]
        public void When_LintCategoryIsAdapted_Expect_StableCode(LintCategory category, string expectedCode)
        {
            var issue = new LintIssue(LintSeverity.Warning, category, "Test diagnostic.");

            Assert.Equal(expectedCode, issue.ToDiagnostic().Code);
        }
    }
}
