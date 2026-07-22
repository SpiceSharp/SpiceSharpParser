using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpiceSharpParser.Diagnostics
{
    /// <summary>
    /// Produces deterministic machine-readable output for structured compilation diagnostics.
    /// </summary>
    public static class SpiceDiagnosticFormatter
    {
        private const string SarifSchema =
            "https://json.schemastore.org/sarif-2.1.0.json";
        private const string ToolInformationUri =
            "https://github.com/SpiceSharp/SpiceSharpParser";

        /// <summary>
        /// Serializes a compilation result as deterministic JSON.
        /// </summary>
        /// <param name="result">The compilation result to serialize.</param>
        /// <returns>A string containing JSON text.</returns>
        public static string ToJson(SpiceCompilationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var writer = new DeterministicJsonWriter();
            writer.StartObject();
            writer.WriteString("schemaVersion", "1.0");
            writer.WriteBoolean("success", result.Success);
            writer.WriteBoolean("policySuccess", result.PolicySuccess);
            writer.WriteString("effectiveDialect", result.EffectiveDialect.ToString());
            writer.WriteNumber("allDiagnosticCount", result.AllDiagnostics.Count);

            writer.WritePropertyName("compatibility");
            writer.StartObject();
            writer.WriteBoolean("canSimulate", result.Compatibility.CanSimulate);
            writer.WriteBoolean("isFullyCompatible", result.Compatibility.IsFullyCompatible);
            writer.WriteNumber("issueCount", result.Compatibility.IssueCount);
            writer.WriteNumber("blockerCount", result.Compatibility.BlockerCount);
            writer.WriteNumber("warningCount", result.Compatibility.WarningCount);
            writer.EndObject();

            WriteDiagnostics(writer, "diagnostics", result.Diagnostics);
            WriteDiagnostics(writer, "allDiagnostics", result.AllDiagnostics);
            WriteDiagnostics(writer, "suppressedDiagnostics", result.SuppressedDiagnostics);
            writer.EndObject();
            return writer.ToString();
        }

        /// <summary>
        /// Serializes effective compilation diagnostics as a deterministic SARIF 2.1.0 log.
        /// Suppressed diagnostics are summarized in run properties and omitted from results.
        /// </summary>
        /// <param name="result">The compilation result to serialize.</param>
        /// <returns>A string containing SARIF JSON text.</returns>
        public static string ToSarif(SpiceCompilationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var writer = new DeterministicJsonWriter();
            writer.StartObject();
            writer.WriteString("$schema", SarifSchema);
            writer.WriteString("version", "2.1.0");
            writer.WritePropertyName("runs");
            writer.StartArray();
            writer.StartObject();

            WriteSarifTool(writer, result.Diagnostics);

            writer.WritePropertyName("results");
            writer.StartArray();
            foreach (SpiceDiagnostic diagnostic in result.Diagnostics)
            {
                WriteSarifResult(writer, diagnostic);
            }

            writer.EndArray();
            writer.WritePropertyName("properties");
            writer.StartObject();
            writer.WriteBoolean("success", result.Success);
            writer.WriteBoolean("policySuccess", result.PolicySuccess);
            writer.WriteString("effectiveDialect", result.EffectiveDialect.ToString());
            writer.WriteNumber("allDiagnosticCount", result.AllDiagnostics.Count);
            writer.WriteNumber("suppressedDiagnosticCount", result.SuppressedDiagnostics.Count);
            writer.EndObject();
            writer.EndObject();
            writer.EndArray();
            writer.EndObject();
            return writer.ToString();
        }

        private static void WriteDiagnostics(
            DeterministicJsonWriter writer,
            string propertyName,
            IEnumerable<SpiceDiagnostic> diagnostics)
        {
            writer.WritePropertyName(propertyName);
            writer.StartArray();
            foreach (SpiceDiagnostic diagnostic in diagnostics)
            {
                WriteDiagnostic(writer, diagnostic);
            }

            writer.EndArray();
        }

        private static void WriteDiagnostic(
            DeterministicJsonWriter writer,
            SpiceDiagnostic diagnostic)
        {
            writer.StartObject();
            writer.WriteString("code", diagnostic.Code);
            writer.WriteString("severity", diagnostic.Severity.ToString());
            writer.WriteString("stage", diagnostic.Stage.ToString());
            writer.WriteString("message", diagnostic.Message);
            writer.WritePropertyName("span");
            WriteSpan(writer, diagnostic.Span);
            writer.WriteString("construct", diagnostic.Construct);
            writer.WriteString("suggestedFix", diagnostic.SuggestedFix);
            writer.WriteString(
                "compatibilityClass",
                diagnostic.CompatibilityClass?.ToString());
            writer.WriteString("helpLink", diagnostic.HelpLink?.AbsoluteUri);

            writer.WritePropertyName("includeStack");
            writer.StartArray();
            foreach (SourceSpan span in diagnostic.IncludeStack)
            {
                WriteSpan(writer, span);
            }

            writer.EndArray();
            writer.WritePropertyName("relatedLocations");
            writer.StartArray();
            foreach (DiagnosticRelatedLocation location in diagnostic.RelatedLocations)
            {
                writer.StartObject();
                writer.WritePropertyName("span");
                WriteSpan(writer, location.Span);
                writer.WriteString("message", location.Message);
                writer.EndObject();
            }

            writer.EndArray();
            writer.EndObject();
        }

        private static void WriteSpan(DeterministicJsonWriter writer, SourceSpan span)
        {
            writer.StartObject();
            writer.WriteString("filePath", span.FilePath);
            writer.WritePropertyName("start");
            WritePosition(writer, span.Start);
            writer.WritePropertyName("end");
            WritePosition(writer, span.End);
            writer.EndObject();
        }

        private static void WritePosition(
            DeterministicJsonWriter writer,
            SourcePosition position)
        {
            writer.StartObject();
            writer.WriteNumber("line", position.Line);
            writer.WriteNumber("column", position.Column);
            writer.EndObject();
        }

        private static void WriteSarifTool(
            DeterministicJsonWriter writer,
            IReadOnlyList<SpiceDiagnostic> diagnostics)
        {
            writer.WritePropertyName("tool");
            writer.StartObject();
            writer.WritePropertyName("driver");
            writer.StartObject();
            writer.WriteString("name", "SpiceSharpParser");
            writer.WriteString("informationUri", ToolInformationUri);
            writer.WritePropertyName("rules");
            writer.StartArray();

            IEnumerable<IGrouping<string, SpiceDiagnostic>> rules = diagnostics
                .GroupBy(diagnostic => diagnostic.Code, StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

            foreach (IGrouping<string, SpiceDiagnostic> rule in rules)
            {
                SpiceDiagnostic representative = rule.First();
                writer.StartObject();
                writer.WriteString("id", representative.Code);
                writer.WriteString("name", GetRuleName(representative.Code));
                writer.WritePropertyName("shortDescription");
                writer.StartObject();
                writer.WriteString("text", GetRuleTitle(representative.Code));
                writer.EndObject();
                if (representative.HelpLink != null)
                {
                    writer.WriteString("helpUri", representative.HelpLink.AbsoluteUri);
                }
                writer.WritePropertyName("defaultConfiguration");
                writer.StartObject();
                writer.WriteString("level", GetSarifLevel(representative.Severity));
                writer.EndObject();
                writer.EndObject();
            }

            writer.EndArray();
            writer.EndObject();
            writer.EndObject();
        }

        private static void WriteSarifResult(
            DeterministicJsonWriter writer,
            SpiceDiagnostic diagnostic)
        {
            writer.StartObject();
            writer.WriteString("ruleId", diagnostic.Code);
            writer.WriteString("level", GetSarifLevel(diagnostic.Severity));
            writer.WritePropertyName("message");
            writer.StartObject();
            writer.WriteString("text", diagnostic.Message);
            writer.EndObject();

            if (!string.IsNullOrEmpty(diagnostic.Span.FilePath) || diagnostic.Span.IsKnown)
            {
                writer.WritePropertyName("locations");
                writer.StartArray();
                WriteSarifLocation(writer, diagnostic.Span);
                writer.EndArray();
            }

            if (diagnostic.RelatedLocations.Count > 0)
            {
                writer.WritePropertyName("relatedLocations");
                writer.StartArray();
                for (int index = 0; index < diagnostic.RelatedLocations.Count; index++)
                {
                    DiagnosticRelatedLocation location = diagnostic.RelatedLocations[index];
                    writer.StartObject();
                    writer.WriteNumber("id", index + 1);
                    WriteSarifPhysicalLocation(writer, location.Span);
                    writer.WritePropertyName("message");
                    writer.StartObject();
                    writer.WriteString("text", location.Message);
                    writer.EndObject();
                    writer.EndObject();
                }

                writer.EndArray();
            }

            writer.WritePropertyName("properties");
            writer.StartObject();
            writer.WriteString("stage", diagnostic.Stage.ToString());
            writer.WriteString("construct", diagnostic.Construct);
            writer.WriteString("suggestedFix", diagnostic.SuggestedFix);
            writer.WriteString(
                "compatibilityClass",
                diagnostic.CompatibilityClass?.ToString());
            writer.WritePropertyName("includeStack");
            writer.StartArray();
            foreach (SourceSpan span in diagnostic.IncludeStack)
            {
                WriteSpan(writer, span);
            }

            writer.EndArray();
            writer.EndObject();
            writer.EndObject();
        }

        private static void WriteSarifLocation(
            DeterministicJsonWriter writer,
            SourceSpan span)
        {
            writer.StartObject();
            WriteSarifPhysicalLocation(writer, span);
            writer.EndObject();
        }

        private static void WriteSarifPhysicalLocation(
            DeterministicJsonWriter writer,
            SourceSpan span)
        {
            writer.WritePropertyName("physicalLocation");
            writer.StartObject();
            if (!string.IsNullOrEmpty(span.FilePath))
            {
                writer.WritePropertyName("artifactLocation");
                writer.StartObject();
                writer.WriteString("uri", GetArtifactUri(span.FilePath));
                writer.EndObject();
            }

            if (span.IsKnown)
            {
                writer.WritePropertyName("region");
                writer.StartObject();
                writer.WriteNumber("startLine", span.Start.Line);
                if (span.Start.HasColumn)
                {
                    writer.WriteNumber("startColumn", span.Start.Column);
                }

                if (span.End.IsKnown)
                {
                    writer.WriteNumber("endLine", span.End.Line);
                }

                if (span.End.HasColumn)
                {
                    writer.WriteNumber("endColumn", span.End.Column);
                }

                writer.EndObject();
            }

            writer.EndObject();
        }

        private static string GetArtifactUri(string filePath)
        {
            if (filePath.Length >= 3
                && char.IsLetter(filePath[0])
                && filePath[1] == ':'
                && (filePath[2] == '\\' || filePath[2] == '/'))
            {
                return new Uri("file:///" + filePath.Replace('\\', '/')).AbsoluteUri;
            }

            if (filePath.StartsWith("\\\\", StringComparison.Ordinal))
            {
                return new Uri("file:" + filePath.Replace('\\', '/')).AbsoluteUri;
            }

            if (filePath[0] == '/')
            {
                return new Uri("file://" + filePath).AbsoluteUri;
            }

            if (Uri.TryCreate(filePath, UriKind.Absolute, out Uri absoluteUri))
            {
                return absoluteUri.AbsoluteUri;
            }

            return string.Join(
                "/",
                filePath
                    .Replace('\\', '/')
                    .Split('/')
                    .Select(Uri.EscapeDataString));
        }

        private static string GetSarifLevel(DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case DiagnosticSeverity.Error:
                    return "error";
                case DiagnosticSeverity.Warning:
                    return "warning";
                case DiagnosticSeverity.Info:
                    return "note";
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown diagnostic severity.");
            }
        }

        private static string GetRuleName(string code)
        {
            return code.StartsWith("SSP", StringComparison.OrdinalIgnoreCase)
                ? "Spice" + code.Substring(3)
                : code;
        }

        private static string GetRuleTitle(string code)
        {
            switch (code)
            {
                case SpiceDiagnosticCodes.LexerError:
                    return "Lexical error";
                case SpiceDiagnosticCodes.ParserError:
                    return "Syntax error";
                case SpiceDiagnosticCodes.PreprocessorError:
                    return "Preprocessing error";
                case SpiceDiagnosticCodes.SourceFileNotFound:
                    return "Source file not found";
                case SpiceDiagnosticCodes.SourceFileReadError:
                    return "Source file cannot be read";
                case SpiceDiagnosticCodes.UnsupportedComponent:
                    return "Unsupported component";
                case SpiceDiagnosticCodes.UnsupportedParameter:
                    return "Unsupported parameter";
                case SpiceDiagnosticCodes.UnsupportedModel:
                    return "Unsupported model";
                case SpiceDiagnosticCodes.UnsupportedControl:
                    return "Unsupported control";
                case SpiceDiagnosticCodes.UnsupportedWaveform:
                    return "Unsupported waveform";
                case SpiceDiagnosticCodes.UnsupportedOption:
                    return "Unsupported option";
                case SpiceDiagnosticCodes.UnsupportedExport:
                    return "Unsupported export";
                case SpiceDiagnosticCodes.UnsupportedSyntax:
                    return "Unsupported syntax";
                case SpiceDiagnosticCodes.ReaderError:
                    return "Translation error";
                case SpiceDiagnosticCodes.FloatingNode:
                    return "Floating node";
                case SpiceDiagnosticCodes.MissingDcPath:
                    return "Missing DC path";
                case SpiceDiagnosticCodes.MissingModel:
                    return "Missing model";
                case SpiceDiagnosticCodes.DuplicateComponent:
                    return "Duplicate component";
                case SpiceDiagnosticCodes.MissingAcMagnitude:
                    return "Missing AC magnitude";
                case SpiceDiagnosticCodes.MissingTranMaxStep:
                    return "Missing transient maximum step";
                case SpiceDiagnosticCodes.EmptyCircuit:
                    return "Empty circuit";
                case SpiceDiagnosticCodes.NoSimulation:
                    return "No simulation";
                case SpiceDiagnosticCodes.NoExports:
                    return "No exports";
                case SpiceDiagnosticCodes.IgnoredConstruct:
                    return "Ignored construct";
                case SpiceDiagnosticCodes.NumericDivergence:
                    return "Numeric compatibility divergence";
                case SpiceDiagnosticCodes.CompatibilityApproximation:
                    return "Compatibility approximation";
                default:
                    return code;
            }
        }

        private sealed class DeterministicJsonWriter
        {
            private readonly StringBuilder _builder = new StringBuilder();
            private readonly Stack<ContainerState> _containers = new Stack<ContainerState>();
            private bool _propertyValuePending;

            public void StartObject()
            {
                BeforeValue();
                _builder.Append('{');
                _containers.Push(new ContainerState());
            }

            public void EndObject()
            {
                _containers.Pop();
                _builder.Append('}');
            }

            public void StartArray()
            {
                BeforeValue();
                _builder.Append('[');
                _containers.Push(new ContainerState());
            }

            public void EndArray()
            {
                _containers.Pop();
                _builder.Append(']');
            }

            public void WritePropertyName(string name)
            {
                BeforeElement();
                AppendQuoted(name);
                _builder.Append(':');
                _propertyValuePending = true;
            }

            public void WriteString(string name, string value)
            {
                WritePropertyName(name);
                WriteStringValue(value);
            }

            public void WriteBoolean(string name, bool value)
            {
                WritePropertyName(name);
                BeforeValue();
                _builder.Append(value ? "true" : "false");
            }

            public void WriteNumber(string name, int value)
            {
                WritePropertyName(name);
                BeforeValue();
                _builder.Append(value.ToString(CultureInfo.InvariantCulture));
            }

            public override string ToString()
            {
                return _builder.ToString();
            }

            private void WriteStringValue(string value)
            {
                BeforeValue();
                if (value == null)
                {
                    _builder.Append("null");
                    return;
                }

                AppendQuoted(value);
            }

            private void BeforeValue()
            {
                if (_propertyValuePending)
                {
                    _propertyValuePending = false;
                    return;
                }

                if (_containers.Count > 0)
                {
                    BeforeElement();
                }
            }

            private void BeforeElement()
            {
                ContainerState state = _containers.Peek();
                if (state.HasElements)
                {
                    _builder.Append(',');
                }
                else
                {
                    state.HasElements = true;
                }
            }

            private void AppendQuoted(string value)
            {
                _builder.Append('"');
                foreach (char character in value)
                {
                    switch (character)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            if (character < ' ' || char.IsSurrogate(character))
                            {
                                _builder.Append("\\u");
                                _builder.Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                _builder.Append(character);
                            }

                            break;
                    }
                }

                _builder.Append('"');
            }

            private sealed class ContainerState
            {
                public bool HasElements { get; set; }
            }
        }
    }
}
