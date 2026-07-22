using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Validation;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Converts existing parser, reader, and linter validation results into structured diagnostics.
    /// </summary>
    public static class SpiceDiagnosticAdapters
    {
        /// <summary>
        /// Converts a legacy validation entry into a structured diagnostic.
        /// </summary>
        public static SpiceDiagnostic ToDiagnostic(this ValidationEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            string message = GetMessage(entry);
            CompatibilityClass? compatibilityClass = GetCompatibilityClass(entry.Source, message);

            return new SpiceDiagnostic(
                GetCode(entry.Source, message),
                GetSeverity(entry.Level),
                GetStage(entry.Source),
                message,
                SourceSpan.FromLineInfo(entry.LineInfo),
                construct: GetConstruct(entry.Source, message),
                suggestedFix: GetSuggestedFix(compatibilityClass),
                compatibilityClass: compatibilityClass);
        }

        /// <summary>
        /// Converts legacy validation entries into structured diagnostics while preserving their order.
        /// </summary>
        public static IReadOnlyList<SpiceDiagnostic> ToDiagnostics(this IEnumerable<ValidationEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            return entries.Select(ToDiagnostic).ToList().AsReadOnly();
        }

        /// <summary>
        /// Converts a linter issue into a structured diagnostic.
        /// </summary>
        public static SpiceDiagnostic ToDiagnostic(this LintIssue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            return new SpiceDiagnostic(
                GetCode(issue.Category),
                GetSeverity(issue.Severity),
                DiagnosticStage.Linter,
                issue.Message,
                construct: issue.NodeOrComponent,
                suggestedFix: issue.SuggestedFix);
        }

        /// <summary>
        /// Converts a linter result into structured diagnostics while preserving issue order.
        /// </summary>
        public static IReadOnlyList<SpiceDiagnostic> ToDiagnostics(this LintResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return result.Issues.Select(ToDiagnostic).ToList().AsReadOnly();
        }

        private static string GetCode(ValidationEntrySource source, string message)
        {
            switch (source)
            {
                case ValidationEntrySource.Lexer:
                    return SpiceDiagnosticCodes.LexerError;
                case ValidationEntrySource.Parser:
                    return SpiceDiagnosticCodes.ParserError;
                case ValidationEntrySource.Processor:
                    return SpiceDiagnosticCodes.PreprocessorError;
                case ValidationEntrySource.Reader:
                    return GetReaderCode(message);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown validation source.");
            }
        }

        private static string GetReaderCode(string message)
        {
            if (StartsWith(message, "Ignored "))
            {
                return SpiceDiagnosticCodes.IgnoredConstruct;
            }

            if (Contains(message, "numeric divergence") || Contains(message, "numerically differs"))
            {
                return SpiceDiagnosticCodes.NumericDivergence;
            }

            if (Contains(message, "approximat") || Contains(message, "lowered to"))
            {
                return SpiceDiagnosticCodes.CompatibilityApproximation;
            }

            if (!IsUnsupported(message))
            {
                return SpiceDiagnosticCodes.ReaderError;
            }

            if (Contains(message, "parameter"))
            {
                return SpiceDiagnosticCodes.UnsupportedParameter;
            }

            if (Contains(message, "model type") || Contains(message, "model level"))
            {
                return SpiceDiagnosticCodes.UnsupportedModel;
            }

            if (Contains(message, "control"))
            {
                return SpiceDiagnosticCodes.UnsupportedControl;
            }

            if (Contains(message, "waveform"))
            {
                return SpiceDiagnosticCodes.UnsupportedWaveform;
            }

            if (Contains(message, "option"))
            {
                return SpiceDiagnosticCodes.UnsupportedOption;
            }

            if (Contains(message, "export"))
            {
                return SpiceDiagnosticCodes.UnsupportedExport;
            }

            if (Contains(message, "component"))
            {
                return SpiceDiagnosticCodes.UnsupportedComponent;
            }

            if (Contains(message, "model"))
            {
                return SpiceDiagnosticCodes.UnsupportedModel;
            }

            return SpiceDiagnosticCodes.UnsupportedSyntax;
        }

        private static CompatibilityClass? GetCompatibilityClass(
            ValidationEntrySource source,
            string message)
        {
            if (source != ValidationEntrySource.Reader)
            {
                return null;
            }

            if (StartsWith(message, "Ignored "))
            {
                return CompatibilityClass.RecognizedNoOp;
            }

            if (Contains(message, "numeric divergence") || Contains(message, "numerically differs"))
            {
                return CompatibilityClass.NumericDivergence;
            }

            if (Contains(message, "approximat") || Contains(message, "lowered to"))
            {
                return CompatibilityClass.ParserShim;
            }

            if (IsUnsupported(message))
            {
                return Contains(message, "engine support")
                    ? CompatibilityClass.EngineRequired
                    : CompatibilityClass.TargetedDiagnostic;
            }

            return null;
        }

        private static string GetConstruct(ValidationEntrySource source, string message)
        {
            if (source != ValidationEntrySource.Reader || string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            int quoteStart = message.IndexOf('\'');
            if (quoteStart >= 0)
            {
                int quoteEnd = message.IndexOf('\'', quoteStart + 1);
                if (quoteEnd > quoteStart + 1)
                {
                    return message.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                }
            }

            int colon = message.IndexOf(':');
            if (colon >= 0 && colon + 1 < message.Length)
            {
                return TrimConstruct(message.Substring(colon + 1));
            }

            const string componentMarker = "Unsupported component ";
            int componentStart = message.IndexOf(componentMarker, StringComparison.OrdinalIgnoreCase);
            if (componentStart >= 0)
            {
                return FirstWord(message.Substring(componentStart + componentMarker.Length));
            }

            return null;
        }

        private static string GetSuggestedFix(CompatibilityClass? compatibilityClass)
        {
            switch (compatibilityClass)
            {
                case CompatibilityClass.RecognizedNoOp:
                    return "Remove this metadata if it is not needed, or leave it in place if the ignored behavior is acceptable.";
                case CompatibilityClass.EngineRequired:
                    return "Replace the construct with an equivalent supported model or provide a custom SpiceSharp implementation.";
                case CompatibilityClass.TargetedDiagnostic:
                case CompatibilityClass.SyntaxAuditGap:
                case CompatibilityClass.ParseOnly:
                    return "Replace the construct with a supported equivalent or register a custom reader mapping.";
                case CompatibilityClass.ParserShim:
                    return "Review the lowered behavior and verify that it matches the source dialect's intent.";
                case CompatibilityClass.NumericDivergence:
                    return "Review the documented numeric difference before relying on simulation results.";
                default:
                    return null;
            }
        }

        private static bool IsUnsupported(string message)
        {
            return Contains(message, "unsupported") || Contains(message, "not supported");
        }

        private static bool StartsWith(string value, string prefix)
        {
            return value?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool Contains(string value, string fragment)
        {
            return value?.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string TrimConstruct(string value)
        {
            string result = value.Trim();
            int locationStart = result.IndexOf(" (at line ", StringComparison.OrdinalIgnoreCase);
            if (locationStart >= 0)
            {
                result = result.Substring(0, locationStart).Trim();
            }

            return result.TrimEnd('.');
        }

        private static string FirstWord(string value)
        {
            string trimmed = value.Trim();
            int end = trimmed.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '.', ':' });
            return end < 0 ? trimmed : trimmed.Substring(0, end);
        }

        private static string GetMessage(ValidationEntry entry)
        {
            string exceptionMessage = entry.Exception?.Message;
            if (string.IsNullOrWhiteSpace(exceptionMessage)
                || entry.Message.IndexOf(exceptionMessage, StringComparison.Ordinal) >= 0)
            {
                return entry.Message;
            }

            return $"{entry.Message} {exceptionMessage}";
        }

        private static DiagnosticStage GetStage(ValidationEntrySource source)
        {
            switch (source)
            {
                case ValidationEntrySource.Lexer:
                    return DiagnosticStage.Lexer;
                case ValidationEntrySource.Parser:
                    return DiagnosticStage.Parser;
                case ValidationEntrySource.Processor:
                    return DiagnosticStage.Preprocessor;
                case ValidationEntrySource.Reader:
                    return DiagnosticStage.Reader;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown validation source.");
            }
        }

        private static DiagnosticSeverity GetSeverity(ValidationEntryLevel level)
        {
            switch (level)
            {
                case ValidationEntryLevel.Error:
                    return DiagnosticSeverity.Error;
                case ValidationEntryLevel.Warning:
                    return DiagnosticSeverity.Warning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown validation level.");
            }
        }

        private static DiagnosticSeverity GetSeverity(LintSeverity severity)
        {
            switch (severity)
            {
                case LintSeverity.Error:
                    return DiagnosticSeverity.Error;
                case LintSeverity.Warning:
                    return DiagnosticSeverity.Warning;
                case LintSeverity.Info:
                    return DiagnosticSeverity.Info;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown lint severity.");
            }
        }

        private static string GetCode(LintCategory category)
        {
            switch (category)
            {
                case LintCategory.FloatingNode:
                    return SpiceDiagnosticCodes.FloatingNode;
                case LintCategory.MissingDCPath:
                    return SpiceDiagnosticCodes.MissingDcPath;
                case LintCategory.MissingModel:
                    return SpiceDiagnosticCodes.MissingModel;
                case LintCategory.DuplicateComponent:
                    return SpiceDiagnosticCodes.DuplicateComponent;
                case LintCategory.MissingACMagnitude:
                    return SpiceDiagnosticCodes.MissingAcMagnitude;
                case LintCategory.MissingTranMaxStep:
                    return SpiceDiagnosticCodes.MissingTranMaxStep;
                case LintCategory.EmptyCircuit:
                    return SpiceDiagnosticCodes.EmptyCircuit;
                case LintCategory.NoSimulation:
                    return SpiceDiagnosticCodes.NoSimulation;
                case LintCategory.NoExports:
                    return SpiceDiagnosticCodes.NoExports;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown lint category.");
            }
        }
    }
}
