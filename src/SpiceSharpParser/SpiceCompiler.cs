using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using SpiceSharpParser.Common;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.Validation;

namespace SpiceSharpParser
{
    /// <summary>
    /// Provides a structured compilation pipeline for user-supplied SPICE netlists.
    /// </summary>
    public static class SpiceCompiler
    {
        /// <summary>
        /// Parses, preprocesses, translates, and optionally lints SPICE source text.
        /// </summary>
        /// <param name="source">SPICE source text.</param>
        /// <param name="options">Compilation options, or null for defaults.</param>
        /// <returns>A structured compilation result.</returns>
        public static SpiceCompilationResult CompileText(string source, SpiceCompileOptions options = null)
        {
            return CompileText(source, null, options);
        }

        /// <summary>
        /// Parses, preprocesses, translates, and optionally lints named SPICE source text.
        /// </summary>
        /// <param name="source">SPICE source text.</param>
        /// <param name="sourceName">Source path or display name used in diagnostics.</param>
        /// <param name="options">Compilation options, or null for defaults.</param>
        /// <returns>A structured compilation result.</returns>
        public static SpiceCompilationResult CompileText(
            string source,
            string sourceName,
            SpiceCompileOptions options = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            options = options ?? new SpiceCompileOptions();
            ValidateOptions(options);

            string workingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory;
            return CompileCore(source, sourceName, workingDirectory, options);
        }

        /// <summary>
        /// Reads, parses, preprocesses, translates, and optionally lints a SPICE source file.
        /// Source-file access failures are returned as diagnostics unless configured to throw.
        /// </summary>
        /// <param name="filePath">Path of the root SPICE source file.</param>
        /// <param name="options">Compilation options, or null for defaults.</param>
        /// <returns>A structured compilation result.</returns>
        public static SpiceCompilationResult CompileFile(string filePath, SpiceCompileOptions options = null)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A source file path is required.", nameof(filePath));
            }

            options = options ?? new SpiceCompileOptions();
            ValidateOptions(options);

            string fullPath = Path.GetFullPath(filePath);
            string source;

            try
            {
                source = File.ReadAllText(fullPath, options.ExternalFilesEncoding);
            }
            catch (FileNotFoundException exception) when (!options.ThrowOnFileAccessError)
            {
                return FileFailure(options, fullPath, SpiceDiagnosticCodes.SourceFileNotFound, exception.Message);
            }
            catch (DirectoryNotFoundException exception) when (!options.ThrowOnFileAccessError)
            {
                return FileFailure(options, fullPath, SpiceDiagnosticCodes.SourceFileNotFound, exception.Message);
            }
            catch (IOException exception) when (!options.ThrowOnFileAccessError)
            {
                return FileFailure(options, fullPath, SpiceDiagnosticCodes.SourceFileReadError, exception.Message);
            }
            catch (UnauthorizedAccessException exception) when (!options.ThrowOnFileAccessError)
            {
                return FileFailure(options, fullPath, SpiceDiagnosticCodes.SourceFileReadError, exception.Message);
            }
            catch (SecurityException exception) when (!options.ThrowOnFileAccessError)
            {
                return FileFailure(options, fullPath, SpiceDiagnosticCodes.SourceFileReadError, exception.Message);
            }

            string workingDirectory = options.WorkingDirectory
                ?? Path.GetDirectoryName(fullPath)
                ?? Environment.CurrentDirectory;

            return CompileCore(source, fullPath, workingDirectory, options);
        }

        private static SpiceCompilationResult CompileCore(
            string source,
            string sourceName,
            string workingDirectory,
            SpiceCompileOptions options)
        {
            var diagnostics = new List<SpiceDiagnostic>();
            CompatibilityOptions compatibility = GetCompatibility(options.Dialect);

            SpiceNetlistParser parser = CreateParser(options, compatibility, workingDirectory);
            SpiceNetlistParseResult parseResult = parser.ParseNetlist(source, sourceName);
            AddDiagnostics(diagnostics, parseResult.ValidationResult.ToDiagnostics(), parseResult.Dependencies);

            if (parseResult.FinalModel == null || (HasErrors(diagnostics) && !options.ContinueAfterErrors))
            {
                return new SpiceCompilationResult(
                    parseResult.InputModel,
                    parseResult.FinalModel,
                    null,
                    diagnostics,
                    options.Dialect,
                    parseResult.Dependencies,
                    options.DiagnosticPolicy);
            }

            SpiceNetlistReaderSettings readerSettings = CreateReaderSettings(
                options,
                parser.Settings.CaseSensitivity,
                compatibility,
                workingDirectory);
            var reader = new SpiceNetlistReader(readerSettings);
            SpiceNetlistReadResult readResult = reader.ReadResult(parseResult.FinalModel);
            SpiceSharpModel translatedModel = readResult.PartialModel;
            AddDiagnostics(diagnostics, readResult.Diagnostics, parseResult.Dependencies);

            if (options.RunLinter && (!HasErrors(diagnostics) || options.ContinueAfterErrors))
            {
                AddDiagnostics(diagnostics, NetlistLinter.Lint(translatedModel).ToDiagnostics(), parseResult.Dependencies);
            }

            return new SpiceCompilationResult(
                parseResult.InputModel,
                parseResult.FinalModel,
                translatedModel,
                diagnostics,
                options.Dialect,
                parseResult.Dependencies,
                options.DiagnosticPolicy);
        }

        private static void AddDiagnostics(
            ICollection<SpiceDiagnostic> destination,
            IEnumerable<SpiceDiagnostic> diagnostics,
            IReadOnlyList<SpiceDependency> dependencies)
        {
            foreach (SpiceDiagnostic diagnostic in diagnostics)
            {
                IReadOnlyList<SourceSpan> includeStack = GetIncludeStack(diagnostic.Span.FilePath, dependencies);
                if (includeStack.Count == 0)
                {
                    destination.Add(diagnostic);
                    continue;
                }

                IEnumerable<DiagnosticRelatedLocation> relatedLocations = diagnostic.RelatedLocations.Concat(
                    includeStack.Select(span => new DiagnosticRelatedLocation(span, "Included or loaded from here.")));

                destination.Add(new SpiceDiagnostic(
                    diagnostic.Code,
                    diagnostic.Severity,
                    diagnostic.Stage,
                    diagnostic.Message,
                    diagnostic.Span,
                    relatedLocations,
                    diagnostic.Construct,
                    diagnostic.SuggestedFix,
                    diagnostic.CompatibilityClass,
                    diagnostic.HelpLink,
                    includeStack));
            }
        }

        private static IReadOnlyList<SourceSpan> GetIncludeStack(
            string sourcePath,
            IReadOnlyList<SpiceDependency> dependencies)
        {
            var result = new List<SourceSpan>();
            if (string.IsNullOrEmpty(sourcePath) || dependencies == null || dependencies.Count == 0)
            {
                return result.AsReadOnly();
            }

            var visited = new HashSet<string>(GetPathComparer());
            string currentPath = sourcePath;

            while (!string.IsNullOrEmpty(currentPath) && visited.Add(currentPath))
            {
                SpiceDependency dependency = dependencies.FirstOrDefault(candidate =>
                    candidate.IsResolved && PathsEqual(candidate.ResolvedPath, currentPath));
                if (dependency == null)
                {
                    break;
                }

                result.Add(dependency.DirectiveSpan);
                currentPath = dependency.SourcePath;
            }

            result.Reverse();
            return result.AsReadOnly();
        }

        private static StringComparer GetPathComparer()
        {
            return Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        }

        private static bool PathsEqual(string left, string right)
        {
            return !string.IsNullOrEmpty(left)
                && !string.IsNullOrEmpty(right)
                && GetPathComparer().Equals(left, right);
        }

        private static SpiceNetlistParser CreateParser(
            SpiceCompileOptions options,
            CompatibilityOptions compatibility,
            string workingDirectory)
        {
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = options.HasTitle;
            parser.Settings.Lexing.EnableBusSyntax = options.EnableBusSyntax;
            parser.Settings.Parsing.IsEndRequired = options.IsEndRequired;
            parser.Settings.ContinueAfterErrors = options.ContinueAfterErrors;
            parser.Settings.MaximumSyntaxErrors = options.MaximumSyntaxErrors;
            parser.Settings.WorkingDirectory = workingDirectory;
            parser.Settings.ExternalFilesEncoding = options.ExternalFilesEncoding;

            if (options.IsNewlineRequired.HasValue)
            {
                parser.Settings.Parsing.IsNewlineRequired = options.IsNewlineRequired.Value;
            }

            CopyCaseSensitivity(options.CaseSensitivity, parser.Settings.CaseSensitivity);
            parser.Settings.Lexing.IsDotStatementNameCaseSensitive = parser.Settings.CaseSensitivity.IsDotStatementNameCaseSensitive;

            options.ConfigureParser?.Invoke(parser.Settings);

            parser.Settings.Compatibility = compatibility;
            parser.Settings.WorkingDirectory = workingDirectory;
            return parser;
        }

        private static SpiceNetlistReaderSettings CreateReaderSettings(
            SpiceCompileOptions options,
            SpiceNetlistCaseSensitivitySettings caseSensitivity,
            CompatibilityOptions compatibility,
            string workingDirectory)
        {
            var settings = new SpiceNetlistReaderSettings(
                caseSensitivity,
                () => workingDirectory,
                options.ExternalFilesEncoding,
                options.Separator,
                options.ExpandSubcircuits)
            {
                Compatibility = compatibility,
                Seed = options.Seed,
            };

            options.ConfigureReader?.Invoke(settings);
            settings.Compatibility = compatibility;
            return settings;
        }

        private static CompatibilityOptions GetCompatibility(SpiceDialect dialect)
        {
            switch (dialect)
            {
                case SpiceDialect.Spice3:
                    return CompatibilityOptions.None;
                case SpiceDialect.PSpice:
                    return CompatibilityOptions.PSpice;
                case SpiceDialect.LTspice:
                    return CompatibilityOptions.LTspice;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialect), dialect, "Unknown SPICE dialect.");
            }
        }

        private static bool HasErrors(IEnumerable<SpiceDiagnostic> diagnostics)
        {
            return diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        }

        private static void ValidateOptions(SpiceCompileOptions options)
        {
            GetCompatibility(options.Dialect);

            if (options.ExternalFilesEncoding == null)
            {
                throw new ArgumentException("ExternalFilesEncoding cannot be null.", nameof(options));
            }

            if (options.CaseSensitivity == null)
            {
                throw new ArgumentException("CaseSensitivity cannot be null.", nameof(options));
            }

            if (options.Separator == null)
            {
                throw new ArgumentException("Separator cannot be null.", nameof(options));
            }

            if (options.MaximumSyntaxErrors <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), "MaximumSyntaxErrors must be positive.");
            }

            if (options.DiagnosticPolicy == null)
            {
                throw new ArgumentException("DiagnosticPolicy cannot be null.", nameof(options));
            }

            options.DiagnosticPolicy.Validate();
        }

        private static SpiceCompilationResult FileFailure(
            SpiceCompileOptions options,
            string fullPath,
            string code,
            string message)
        {
            var diagnostic = new SpiceDiagnostic(
                code,
                DiagnosticSeverity.Error,
                DiagnosticStage.Preprocessor,
                message,
                new SourceSpan(fullPath, SourcePosition.Unknown, SourcePosition.Unknown),
                construct: fullPath,
                suggestedFix: "Verify that the source path exists and is readable.");

            return new SpiceCompilationResult(
                null,
                null,
                null,
                new[] { diagnostic },
                options.Dialect,
                diagnosticPolicy: options.DiagnosticPolicy);
        }

        private static void CopyCaseSensitivity(
            SpiceNetlistCaseSensitivitySettings source,
            SpiceNetlistCaseSensitivitySettings destination)
        {
            destination.IsDistributionNameCaseSensitive = source.IsDistributionNameCaseSensitive;
            destination.IsDotStatementNameCaseSensitive = source.IsDotStatementNameCaseSensitive;
            destination.IsEntityNamesCaseSensitive = source.IsEntityNamesCaseSensitive;
            destination.IsExpressionNameCaseSensitive = source.IsExpressionNameCaseSensitive;
            destination.IsFunctionNameCaseSensitive = source.IsFunctionNameCaseSensitive;
            destination.IsModelTypeCaseSensitive = source.IsModelTypeCaseSensitive;
            destination.IsNodeNameCaseSensitive = source.IsNodeNameCaseSensitive;
            destination.IsParameterNameCaseSensitive = source.IsParameterNameCaseSensitive;
            destination.IsSubcircuitNameCaseSensitive = source.IsSubcircuitNameCaseSensitive;
        }
    }
}
