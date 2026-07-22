using System;
using System.IO;
using System.Linq;
using SpiceSharpParser.Diagnostics;
using Xunit;

namespace SpiceSharpParser.Tests.Compilation
{
    public class SpiceCompilerTests
    {
        [Fact]
        public void CompileText_WhenNetlistIsValid_ReturnsAllModelsAndNoErrors()
        {
            SpiceCompilationResult result = SpiceCompiler.CompileText(ValidNetlist());

            Assert.True(result.Success);
            Assert.NotNull(result.InputModel);
            Assert.NotNull(result.ExpandedModel);
            Assert.NotNull(result.Model);
            Assert.Same(result.Model, result.TranslatedModel);
            Assert.DoesNotContain(result.Diagnostics, IsError);
            Assert.Equal(SpiceDialect.Spice3, result.EffectiveDialect);
        }

        [Fact]
        public void CompileText_WhenParserFails_ReturnsDiagnosticInsteadOfThrowing()
        {
            string source = Lines(
                "compiler parser failure",
                "R1 out 0 1k");

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "memory://broken.cir");

            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.False(result.Success);
            Assert.Null(result.Model);
            Assert.Null(result.TranslatedModel);
            Assert.Equal(SpiceDiagnosticCodes.ParserError, diagnostic.Code);
            Assert.Equal(DiagnosticStage.Parser, diagnostic.Stage);
            Assert.Contains("No .END", diagnostic.Message);
            Assert.Equal("memory://broken.cir", diagnostic.Span.FilePath);
            Assert.True(diagnostic.Span.Start.Line > 0);
        }

        [Fact]
        public void CompileText_WhenIndependentParserErrorsExist_RecoversAndReturnsAllLocations()
        {
            string source = Lines(
                "compiler parser recovery",
                "  =",
                "V1 out 0 1",
                "  ,",
                "R1 out 0 1k",
                ".op",
                ".end");
            var options = new SpiceCompileOptions { RunLinter = false };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "syntax.net", options);

            SpiceDiagnostic[] diagnostics = result.Diagnostics
                .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Parser)
                .ToArray();
            Assert.Collection(
                diagnostics,
                first =>
                {
                    Assert.Equal(2, first.Span.Start.Line);
                    Assert.Equal(3, first.Span.Start.Column);
                    Assert.Equal(4, first.Span.End.Column);
                },
                second =>
                {
                    Assert.Equal(4, second.Span.Start.Line);
                    Assert.Equal(3, second.Span.Start.Column);
                    Assert.Equal(4, second.Span.End.Column);
                });
            Assert.All(diagnostics, diagnostic => Assert.Equal("syntax.net", diagnostic.Span.FilePath));
            Assert.NotNull(result.InputModel);
            Assert.NotNull(result.ExpandedModel);
            Assert.NotNull(result.TranslatedModel);
            Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "V1");
            Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "R1");
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenIndependentLexerErrorsExist_RecoversAndReturnsAllLocations()
        {
            string source = Lines(
                "compiler lexer recovery",
                "Rbad out 0 1k}",
                "V1 out 0 1",
                "Cbad out 0 1u}",
                "R1 out 0 1k",
                ".op",
                ".end");
            var options = new SpiceCompileOptions { RunLinter = false };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "lexer.net", options);

            SpiceDiagnostic[] diagnostics = result.Diagnostics
                .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Lexer)
                .ToArray();
            Assert.Collection(
                diagnostics,
                first => Assert.Equal(2, first.Span.Start.Line),
                second => Assert.Equal(4, second.Span.Start.Line));
            Assert.All(diagnostics, diagnostic =>
            {
                Assert.Equal("lexer.net", diagnostic.Span.FilePath);
                Assert.True(diagnostic.Span.Start.HasColumn);
                Assert.True(diagnostic.Span.End.Column > diagnostic.Span.Start.Column);
            });
            Assert.NotNull(result.InputModel);
            Assert.NotNull(result.TranslatedModel);
            Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "V1");
            Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "R1");
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenLexerAndParserErrorsExist_ReturnsDiagnosticsInPipelineOrder()
        {
            string source = Lines(
                "compiler mixed syntax recovery",
                "Rbad out 0 1k}",
                "V1 out 0 1",
                "=",
                "R1 out 0 1k",
                ".op",
                ".end");
            var options = new SpiceCompileOptions { RunLinter = false };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "mixed.net", options);

            Assert.Collection(
                result.Diagnostics,
                lexer =>
                {
                    Assert.Equal(DiagnosticStage.Lexer, lexer.Stage);
                    Assert.Equal(2, lexer.Span.Start.Line);
                },
                parser =>
                {
                    Assert.Equal(DiagnosticStage.Parser, parser.Stage);
                    Assert.Equal(4, parser.Span.Start.Line);
                });
            Assert.NotNull(result.InputModel);
            Assert.NotNull(result.TranslatedModel);
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenParserRecoveryIsDisabled_StopsAtFirstSyntaxError()
        {
            string source = Lines(
                "compiler parser stop",
                "=",
                "V1 out 0 1",
                ",",
                ".end");
            var options = new SpiceCompileOptions
            {
                ContinueAfterErrors = false,
                RunLinter = false,
            };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "stop.net", options);

            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(DiagnosticStage.Parser, diagnostic.Stage);
            Assert.Equal(2, diagnostic.Span.Start.Line);
            Assert.Null(result.InputModel);
            Assert.Null(result.TranslatedModel);
        }

        [Fact]
        public void CompileText_WhenSyntaxErrorLimitIsReached_StopsRecoveryAtTheLimit()
        {
            string source = Lines(
                "compiler parser cap",
                "=",
                "V1 out 0 1",
                ",",
                ".end");
            var options = new SpiceCompileOptions
            {
                MaximumSyntaxErrors = 1,
                RunLinter = false,
            };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "cap.net", options);

            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(DiagnosticStage.Parser, diagnostic.Stage);
            Assert.Equal(2, diagnostic.Span.Start.Line);
            Assert.Null(result.InputModel);
            Assert.Null(result.TranslatedModel);
        }

        [Fact]
        public void CompileText_WhenReaderFindsMultipleErrors_ReturnsAllReaderDiagnostics()
        {
            string source = Lines(
                "compiler reader aggregation",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".backanno",
                ".options plotwinsize=0",
                ".op",
                ".save V(out)",
                ".end");
            var options = new SpiceCompileOptions { RunLinter = false };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "vendor.net", options);

            SpiceDiagnostic[] readerErrors = result.Diagnostics
                .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Reader && IsError(diagnostic))
                .ToArray();

            Assert.True(readerErrors.Length >= 2);
            Assert.All(readerErrors, diagnostic => Assert.Equal("vendor.net", diagnostic.Span.FilePath));
            Assert.Null(result.Model);
            Assert.NotNull(result.TranslatedModel);
        }

        [Fact]
        public void CompileText_WhenIncludeIsMissing_ReturnsPreprocessorDiagnosticWithRootLocation()
        {
            string workingDirectory = CreateTempDirectory();

            try
            {
                string source = Lines(
                    "compiler missing include",
                    ".include missing.inc",
                    "V1 out 0 1",
                    "R1 out 0 1k",
                    ".op",
                    ".save V(out)",
                    ".end");
                var options = new SpiceCompileOptions
                {
                    WorkingDirectory = workingDirectory,
                    RunLinter = false,
                };

                SpiceCompilationResult result = SpiceCompiler.CompileText(source, "root.cir", options);

                SpiceDiagnostic diagnostic = Assert.Single(
                    result.Diagnostics,
                    item => item.Stage == DiagnosticStage.Preprocessor);
                Assert.Equal(SpiceDiagnosticCodes.PreprocessorError, diagnostic.Code);
                Assert.Equal("root.cir", diagnostic.Span.FilePath);
                Assert.Equal(2, diagnostic.Span.Start.Line);
                Assert.NotNull(result.ExpandedModel);
                Assert.Null(result.Model);
            }
            finally
            {
                Directory.Delete(workingDirectory, true);
            }
        }

        [Fact]
        public void CompileText_WhenLtspiceDialectIsSelected_AppliesItToReader()
        {
            string source = Lines(
                "compiler LTspice",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".backanno",
                ".op",
                ".save V(out)",
                ".end");
            var options = new SpiceCompileOptions
            {
                Dialect = SpiceDialect.LTspice,
                ConfigureParser = settings => settings.Compatibility = CompatibilityOptions.None,
                ConfigureReader = settings => settings.Compatibility = CompatibilityOptions.None,
            };

            SpiceCompilationResult result = SpiceCompiler.CompileText(source, "ltspice.net", options);

            SpiceDiagnostic warning = Assert.Single(
                result.Diagnostics,
                diagnostic => diagnostic.Stage == DiagnosticStage.Reader);
            Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
            Assert.Equal("ltspice.net", warning.Span.FilePath);
            Assert.Equal(4, warning.Span.Start.Line);
            Assert.True(result.Success);
            Assert.NotNull(result.Model);
            Assert.Equal(SpiceDialect.LTspice, result.EffectiveDialect);
        }

        [Fact]
        public void CompileText_WhenContinuationIsEnabled_AggregatesReaderAndLinterErrors()
        {
            SpiceCompilationResult result = SpiceCompiler.CompileText(
                NetlistWithReaderAndLintErrors(),
                new SpiceCompileOptions { ContinueAfterErrors = true });

            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Stage == DiagnosticStage.Reader && IsError(diagnostic));
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == SpiceDiagnosticCodes.MissingDcPath);
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenContinuationIsDisabled_StopsBeforeLinterAfterReaderError()
        {
            SpiceCompilationResult result = SpiceCompiler.CompileText(
                NetlistWithReaderAndLintErrors(),
                new SpiceCompileOptions { ContinueAfterErrors = false });

            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Stage == DiagnosticStage.Reader && IsError(diagnostic));
            Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Stage == DiagnosticStage.Linter);
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileText_WhenLinterFindsBlockingIssue_DoesNotExposePartialModel()
        {
            string source = Lines(
                "compiler linter gating",
                "C1 floating 0 1u",
                ".op",
                ".end");

            SpiceCompilationResult result = SpiceCompiler.CompileText(source);

            Assert.NotNull(result.InputModel);
            Assert.NotNull(result.ExpandedModel);
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == SpiceDiagnosticCodes.MissingDcPath);
            Assert.Null(result.Model);
            Assert.NotNull(result.TranslatedModel);
            Assert.False(result.Success);
        }

        [Fact]
        public void CompileFile_WhenWorkingDirectoryIsNotSpecified_ResolvesRelativeIncludesFromSourceDirectory()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");

            try
            {
                File.WriteAllText(Path.Combine(directory, "load.inc"), "R1 out 0 1k" + Environment.NewLine);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler file",
                        ".include load.inc",
                        "V1 out 0 1",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(sourcePath);

                Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
                Assert.NotNull(result.Model);
                Assert.Contains(result.Model.Circuit, entity => entity.Name == "R1");
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenNestedIncludeProducesDiagnostic_ReturnsDependenciesAndRootToLeafIncludeStack()
        {
            string directory = CreateTempDirectory();
            string nestedDirectory = Path.Combine(directory, "nested");
            string sourcePath = Path.Combine(directory, "root.cir");
            string parentPath = Path.Combine(nestedDirectory, "parent.inc");
            string childPath = Path.Combine(nestedDirectory, "child.inc");

            try
            {
                Directory.CreateDirectory(nestedDirectory);
                File.WriteAllText(parentPath, ".include child.inc" + Environment.NewLine);
                File.WriteAllText(childPath, ".backanno" + Environment.NewLine);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler nested dependency",
                        ".include nested/parent.inc",
                        "V1 out 0 1",
                        "R1 out 0 1k",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions { RunLinter = false });

                Assert.Collection(
                    result.Dependencies,
                    parent => AssertDependency(
                        parent,
                        SpiceDependencyKind.Include,
                        "nested/parent.inc",
                        parentPath,
                        sourcePath,
                        2),
                    child => AssertDependency(
                        child,
                        SpiceDependencyKind.Include,
                        "child.inc",
                        childPath,
                        parentPath,
                        1));

                SpiceDiagnostic diagnostic = Assert.Single(
                    result.Diagnostics,
                    item => item.Stage == DiagnosticStage.Reader);
                Assert.Equal(childPath, diagnostic.Span.FilePath);
                Assert.Collection(
                    diagnostic.IncludeStack,
                    rootInclude => AssertSpan(rootInclude, sourcePath, 2),
                    nestedInclude => AssertSpan(nestedInclude, parentPath, 1));
                Assert.Equal(2, diagnostic.RelatedLocations.Count);
                Assert.Null(result.Model);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenNestedDependencyIsMissing_RecordsFailureAndParentAncestry()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string parentPath = Path.Combine(directory, "parent.inc");
            string missingPath = Path.Combine(directory, "missing.inc");

            try
            {
                File.WriteAllText(parentPath, ".include missing.inc" + Environment.NewLine);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler missing nested dependency",
                        ".include parent.inc",
                        "V1 out 0 1",
                        "R1 out 0 1k",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions { RunLinter = false });

                Assert.Equal(2, result.Dependencies.Count);
                Assert.Equal(SpiceDependencyStatus.Resolved, result.Dependencies[0].Status);
                SpiceDependency missing = result.Dependencies[1];
                Assert.Equal(SpiceDependencyStatus.NotFound, missing.Status);
                Assert.Equal(missingPath, missing.ResolvedPath);
                Assert.Equal(parentPath, missing.SourcePath);

                SpiceDiagnostic diagnostic = Assert.Single(
                    result.Diagnostics,
                    item => item.Stage == DiagnosticStage.Preprocessor);
                Assert.Equal(parentPath, diagnostic.Span.FilePath);
                SourceSpan includeSite = Assert.Single(diagnostic.IncludeStack);
                AssertSpan(includeSite, sourcePath, 2);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenLibrarySectionIsLoaded_RecordsLibraryDependencyAndSection()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string libraryPath = Path.Combine(directory, "models.lib");

            try
            {
                string librarySource = Lines(
                    ".lib passive",
                    "R1 out 0 1k",
                    ".endl") + Environment.NewLine;
                File.WriteAllText(libraryPath, librarySource);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler library dependency",
                        ".lib models.lib passive",
                        "V1 out 0 1",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(sourcePath);

                SpiceDependency dependency = Assert.Single(result.Dependencies);
                AssertDependency(
                    dependency,
                    SpiceDependencyKind.Library,
                    "models.lib",
                    libraryPath,
                    sourcePath,
                    2);
                Assert.Equal("passive", dependency.LibrarySection);
                Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenIncludedLibraryHasSyntaxError_AttributesDiagnosticToLibraryWithAncestry()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string libraryPath = Path.Combine(directory, "broken.lib");

            try
            {
                File.WriteAllText(
                    libraryPath,
                    Lines(
                        ".lib passive",
                        "R1 out 0 1k",
                        ".endl"));
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler broken library",
                        ".lib broken.lib passive",
                        "V1 out 0 1",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(sourcePath);

                SpiceDependency dependency = Assert.Single(result.Dependencies);
                Assert.Equal(SpiceDependencyStatus.Resolved, dependency.Status);
                SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
                Assert.Equal(DiagnosticStage.Parser, diagnostic.Stage);
                Assert.Equal(libraryPath, diagnostic.Span.FilePath);
                SourceSpan includeSite = Assert.Single(diagnostic.IncludeStack);
                AssertSpan(includeSite, sourcePath, 2);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenLibraryIsMissing_ReturnsNotFoundDependencyWithoutRetrying()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string missingPath = Path.Combine(directory, "missing.lib");

            try
            {
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler missing library",
                        ".lib missing.lib passive",
                        "V1 out 0 1",
                        "R1 out 0 1k",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(sourcePath);

                SpiceDependency dependency = Assert.Single(result.Dependencies);
                Assert.Equal(SpiceDependencyKind.Library, dependency.Kind);
                Assert.Equal(SpiceDependencyStatus.NotFound, dependency.Status);
                Assert.Equal(missingPath, dependency.ResolvedPath);
                Assert.Contains(
                    result.Diagnostics,
                    diagnostic => diagnostic.Stage == DiagnosticStage.Preprocessor && IsError(diagnostic));
                Assert.Null(result.Model);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenLibrarySectionHasNoEndl_ReturnsLocatedPreprocessorDiagnostic()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string libraryPath = Path.Combine(directory, "broken.lib");

            try
            {
                string librarySource = Lines(
                    ".lib passive",
                    "R1 out 0 1k") + Environment.NewLine;
                File.WriteAllText(libraryPath, librarySource);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler library without endl",
                        ".lib broken.lib passive",
                        "V1 out 0 1",
                        ".op",
                        ".save V(out)",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(sourcePath);

                SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
                Assert.Equal(DiagnosticStage.Preprocessor, diagnostic.Stage);
                Assert.Contains(".ENDL", diagnostic.Message);
                Assert.Equal(libraryPath, diagnostic.Span.FilePath);
                SourceSpan includeSite = Assert.Single(diagnostic.IncludeStack);
                AssertSpan(includeSite, sourcePath, 2);
                Assert.Null(result.Model);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenNestedIncludeHasMultipleSyntaxErrors_RecoversWithCompleteAncestry()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string parentPath = Path.Combine(directory, "parent.inc");
            string childPath = Path.Combine(directory, "child.inc");

            try
            {
                File.WriteAllText(parentPath, ".include child.inc" + Environment.NewLine);
                string childSource = Lines(
                    "=",
                    "Vchild out 0 1",
                    ",",
                    "Rchild out 0 1k") + Environment.NewLine;
                File.WriteAllText(childPath, childSource);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler nested syntax recovery",
                        ".include parent.inc",
                        ".op",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions { RunLinter = false });

                SpiceDiagnostic[] diagnostics = result.Diagnostics
                    .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Parser)
                    .ToArray();
                Assert.Collection(
                    diagnostics,
                    first => Assert.Equal(1, first.Span.Start.Line),
                    second => Assert.Equal(3, second.Span.Start.Line));
                Assert.All(diagnostics, diagnostic =>
                {
                    Assert.Equal(childPath, diagnostic.Span.FilePath);
                    Assert.Collection(
                        diagnostic.IncludeStack,
                        rootInclude => AssertSpan(rootInclude, sourcePath, 2),
                        nestedInclude => AssertSpan(nestedInclude, parentPath, 1));
                });
                Assert.NotNull(result.ExpandedModel);
                Assert.NotNull(result.TranslatedModel);
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Vchild");
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rchild");
                Assert.Null(result.Model);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenSiblingIncludesHaveErrors_RecoversEachAndKeepsTheirValidStatements()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string firstPath = Path.Combine(directory, "first.inc");
            string secondPath = Path.Combine(directory, "second.inc");

            try
            {
                File.WriteAllText(firstPath, Lines("Rbad out 0 1k}", "Rfirst out 0 1k") + Environment.NewLine);
                File.WriteAllText(secondPath, Lines(",", "Rsecond out 0 2k") + Environment.NewLine);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler sibling syntax recovery",
                        ".include first.inc",
                        ".include second.inc",
                        "V1 out 0 1",
                        ".op",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions { RunLinter = false });

                SpiceDiagnostic[] diagnostics = result.Diagnostics
                    .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Lexer
                        || diagnostic.Stage == DiagnosticStage.Parser)
                    .ToArray();
                Assert.Collection(
                    diagnostics,
                    first =>
                    {
                        Assert.Equal(DiagnosticStage.Lexer, first.Stage);
                        Assert.Equal(firstPath, first.Span.FilePath);
                        AssertSpan(Assert.Single(first.IncludeStack), sourcePath, 2);
                    },
                    second =>
                    {
                        Assert.Equal(DiagnosticStage.Parser, second.Stage);
                        Assert.Equal(secondPath, second.Span.FilePath);
                        AssertSpan(Assert.Single(second.IncludeStack), sourcePath, 3);
                    });
                Assert.NotNull(result.TranslatedModel);
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rfirst");
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rsecond");
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenGlobalSyntaxLimitIsReached_StillProcessesValidSiblingDependencies()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");

            try
            {
                File.WriteAllText(Path.Combine(directory, "first.inc"), Lines("=", "Rfirst out 0 1k") + Environment.NewLine);
                File.WriteAllText(Path.Combine(directory, "second.inc"), Lines(",", "Rsecond out 0 2k") + Environment.NewLine);
                File.WriteAllText(Path.Combine(directory, "third.inc"), Lines("=", "Rthird out 0 3k") + Environment.NewLine);
                File.WriteAllText(Path.Combine(directory, "good.inc"), "Rgood out 0 4k" + Environment.NewLine);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler global syntax cap",
                        ".include first.inc",
                        ".include second.inc",
                        ".include third.inc",
                        ".include good.inc",
                        "V1 out 0 1",
                        ".op",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions
                    {
                        MaximumSyntaxErrors = 2,
                        RunLinter = false,
                    });

                SpiceDiagnostic[] syntaxDiagnostics = result.Diagnostics
                    .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Lexer
                        || diagnostic.Stage == DiagnosticStage.Parser)
                    .ToArray();
                Assert.Equal(2, syntaxDiagnostics.Length);
                Assert.NotNull(result.TranslatedModel);
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rfirst");
                Assert.DoesNotContain(result.TranslatedModel.Circuit, entity => entity.Name == "Rsecond");
                Assert.DoesNotContain(result.TranslatedModel.Circuit, entity => entity.Name == "Rthird");
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rgood");
                Assert.Equal(4, result.Dependencies.Count);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenLibraryHasMultipleSyntaxErrors_RecoversSelectedSection()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string libraryPath = Path.Combine(directory, "models.lib");

            try
            {
                string librarySource = Lines(
                    ".lib passive",
                    "=",
                    "Rlib out 0 1k",
                    ",",
                    "Clib out 0 1u",
                    ".endl") + Environment.NewLine;
                File.WriteAllText(libraryPath, librarySource);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler library syntax recovery",
                        ".lib models.lib passive",
                        "V1 out 0 1",
                        ".op",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions { RunLinter = false });

                SpiceDiagnostic[] diagnostics = result.Diagnostics
                    .Where(diagnostic => diagnostic.Stage == DiagnosticStage.Parser)
                    .ToArray();
                Assert.Collection(
                    diagnostics,
                    first => Assert.Equal(2, first.Span.Start.Line),
                    second => Assert.Equal(4, second.Span.Start.Line));
                Assert.All(diagnostics, diagnostic =>
                {
                    Assert.Equal(libraryPath, diagnostic.Span.FilePath);
                    AssertSpan(Assert.Single(diagnostic.IncludeStack), sourcePath, 2);
                });
                Assert.NotNull(result.TranslatedModel);
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rlib");
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Clib");
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenOneLibraryHasNoEndl_ContinuesWithSiblingLibrary()
        {
            string directory = CreateTempDirectory();
            string sourcePath = Path.Combine(directory, "root.cir");
            string brokenPath = Path.Combine(directory, "broken.lib");
            string validPath = Path.Combine(directory, "valid.lib");

            try
            {
                File.WriteAllText(brokenPath, Lines(".lib passive", "Rbroken out 0 1k") + Environment.NewLine);
                File.WriteAllText(validPath, Lines(".lib passive", "Rvalid out 0 2k", ".endl") + Environment.NewLine);
                File.WriteAllText(
                    sourcePath,
                    Lines(
                        "compiler structural library recovery",
                        ".lib broken.lib passive",
                        ".lib valid.lib passive",
                        "V1 out 0 1",
                        ".op",
                        ".end"));

                SpiceCompilationResult result = SpiceCompiler.CompileFile(
                    sourcePath,
                    new SpiceCompileOptions { RunLinter = false });

                SpiceDiagnostic diagnostic = Assert.Single(
                    result.Diagnostics,
                    item => item.Stage == DiagnosticStage.Preprocessor);
                Assert.Contains(".ENDL", diagnostic.Message);
                Assert.Equal(brokenPath, diagnostic.Span.FilePath);
                AssertSpan(Assert.Single(diagnostic.IncludeStack), sourcePath, 2);
                Assert.NotNull(result.ExpandedModel);
                Assert.NotNull(result.TranslatedModel);
                Assert.DoesNotContain(result.TranslatedModel.Circuit, entity => entity.Name == "Rbroken");
                Assert.Contains(result.TranslatedModel.Circuit, entity => entity.Name == "Rvalid");
                Assert.Equal(2, result.Dependencies.Count);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public void CompileFile_WhenSourceDoesNotExist_ReturnsStableDiagnostic()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.cir");

            SpiceCompilationResult result = SpiceCompiler.CompileFile(path);

            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(SpiceDiagnosticCodes.SourceFileNotFound, diagnostic.Code);
            Assert.Equal(Path.GetFullPath(path), diagnostic.Span.FilePath);
            Assert.Null(result.Model);
        }

        [Fact]
        public void CompileFile_WhenSourceDoesNotExistAndThrowIsEnabled_Throws()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.cir");
            var options = new SpiceCompileOptions { ThrowOnFileAccessError = true };

            Assert.Throws<DirectoryNotFoundException>(() => SpiceCompiler.CompileFile(path, options));
        }

        [Fact]
        public void CompileMethods_WhenArgumentsAreApiMisuse_ThrowArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => SpiceCompiler.CompileText(null));
            Assert.Throws<ArgumentNullException>(() => SpiceCompiler.CompileFile(null));
            Assert.Throws<ArgumentException>(() => SpiceCompiler.CompileFile(" "));
            Assert.Throws<ArgumentOutOfRangeException>(() => SpiceCompiler.CompileText(
                ValidNetlist(),
                new SpiceCompileOptions { Dialect = (SpiceDialect)999 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => SpiceCompiler.CompileText(
                ValidNetlist(),
                new SpiceCompileOptions { MaximumSyntaxErrors = 0 }));
        }

        private static bool IsError(SpiceDiagnostic diagnostic)
        {
            return diagnostic.Severity == DiagnosticSeverity.Error;
        }

        private static void AssertDependency(
            SpiceDependency dependency,
            SpiceDependencyKind kind,
            string requestedPath,
            string resolvedPath,
            string sourcePath,
            int line)
        {
            Assert.Equal(kind, dependency.Kind);
            Assert.Equal(requestedPath, dependency.RequestedPath);
            Assert.Equal(resolvedPath, dependency.ResolvedPath);
            Assert.Equal(SpiceDependencyStatus.Resolved, dependency.Status);
            Assert.True(dependency.IsResolved);
            AssertSpan(dependency.DirectiveSpan, sourcePath, line);
        }

        private static void AssertSpan(SourceSpan span, string filePath, int line)
        {
            Assert.Equal(filePath, span.FilePath);
            Assert.Equal(line, span.Start.Line);
        }

        private static string ValidNetlist()
        {
            return Lines(
                "compiler success",
                "V1 out 0 1",
                "R1 out 0 1k",
                ".op",
                ".save V(out)",
                ".end");
        }

        private static string NetlistWithReaderAndLintErrors()
        {
            return Lines(
                "compiler continuation",
                "V1 out 0 1",
                "R1 out 0 1k",
                "C1 floating 0 1u",
                ".backanno",
                ".op",
                ".save V(out)",
                ".end");
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "SpiceCompilerTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
