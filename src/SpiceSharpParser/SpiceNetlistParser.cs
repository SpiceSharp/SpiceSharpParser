using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Common.Processors;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Netlist;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// The SPICE netlist parser.
    /// </summary>
    public class SpiceNetlistParser
    {
        private readonly List<SpiceDependency> _dependencies = new List<SpiceDependency>();
        private ValidationEntryCollection _activeValidation;
        private int _syntaxErrorCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistParser"/> class.
        /// </summary>
        public SpiceNetlistParser()
            : this(new SpiceNetlistParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistParser"/> class.
        /// </summary>
        /// <param name="settings">SPICE parser settings.</param>
        public SpiceNetlistParser(SpiceNetlistParserSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SingleNetlistParser = new SingleSpiceNetlistParser(Settings.Parsing);

            TokenProviderPool = new SpiceTokenProviderPool();
            var includesPreprocessor = new IncludesProcessor(
                new FileReader(() => Settings.ExternalFilesEncoding),
                TokenProviderPool,
                SingleNetlistParser,
                () => Settings.WorkingDirectory,
                Settings.Lexing);

            var libPreprocessor = new LibProcessor(
                new FileReader(() => Settings.ExternalFilesEncoding),
                TokenProviderPool,
                SingleNetlistParser,
                includesPreprocessor,
                () => Settings.WorkingDirectory,
                Settings.Lexing);

            includesPreprocessor.DependencyRecorder = RecordDependency;
            libPreprocessor.DependencyRecorder = RecordDependency;
            includesPreprocessor.SourceParser = ParseExternalNetlist;
            libPreprocessor.SourceParser = ParseExternalNetlist;

            var appendModelPreprocessor = new AppendModelProcessor();
            var akoModelPreprocessor = new AkoModelProcessor();
            var sweepsPreprocessor = new SweepsProcessor();
            var ifPostprocessor = new IfProcessor();
            var macroPreprocessor = new MacroProcessor();

            Processors.AddRange(new IProcessor[] { includesPreprocessor, libPreprocessor, macroPreprocessor, appendModelPreprocessor, akoModelPreprocessor, sweepsPreprocessor, ifPostprocessor });
        }

        public ISpiceTokenProviderPool TokenProviderPool { get; set; }

        /// <summary>
        /// Gets the parser parserSettings.
        /// </summary>
        public SpiceNetlistParserSettings Settings { get; }

        /// <summary>
        /// Gets the preprocessors.
        /// </summary>
        public List<IProcessor> Processors { get; } = new List<IProcessor>();

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        protected ISingleSpiceNetlistParser SingleNetlistParser { get; }

        /// <summary>
        /// Parses the netlist.
        /// </summary>
        /// <param name="spiceNetlist">Netlist to parse.</param>
        /// <returns>
        /// A parsing result.
        /// </returns>
        public SpiceNetlistParseResult ParseNetlist(string spiceNetlist)
        {
            return ParseNetlist(spiceNetlist, null);
        }

        /// <summary>
        /// Parses the netlist and associates root tokens with a source name.
        /// </summary>
        /// <param name="spiceNetlist">Netlist to parse.</param>
        /// <param name="sourceName">Source path or display name for root-netlist diagnostics.</param>
        /// <returns>
        /// A parsing result.
        /// </returns>
        public SpiceNetlistParseResult ParseNetlist(string spiceNetlist, string sourceName)
        {
            if (spiceNetlist == null)
            {
                throw new ArgumentNullException(nameof(spiceNetlist));
            }

            _dependencies.Clear();
            var result = new SpiceNetlistParseResult { ValidationResult = new ValidationEntryCollection() };
            _activeValidation = result.ValidationResult;
            _syntaxErrorCount = 0;
            Settings.Lexing.Compatibility = Settings.Compatibility;

            try
            {
                SpiceToken[] tokens = TokenizeWithRecovery(
                    spiceNetlist,
                    sourceName,
                    Settings.Lexing);
                if (tokens != null)
                {
                    SpiceNetlist originalNetlistModel = ParseWithRecovery(
                        tokens,
                        sourceName,
                        SingleNetlistParser);
                    if (originalNetlistModel != null)
                    {
                        result.InputModel = originalNetlistModel;
                        result.FinalModel = GetFinalModel(originalNetlistModel, result.ValidationResult);
                    }
                }
            }
            catch (LexerException e)
            {
                SetSourceName(e.LineInfo, sourceName);
                result.ValidationResult.AddError(
                    ValidationEntrySource.Lexer,
                    e.Message,
                    e.LineInfo,
                    e);
            }
            catch (ParseException e)
            {
                SetSourceName(e.LineInfo, sourceName);
                result.ValidationResult.AddError(
                    ValidationEntrySource.Parser,
                    e.Message,
                    e.LineInfo,
                    e);
            }
            catch (Common.SpiceSharpParserException e)
            {
                SetSourceName(e.LineInfo, sourceName);
                result.ValidationResult.AddError(
                    ValidationEntrySource.Processor,
                    e.Message,
                    e.LineInfo,
                    e);
            }
            catch (Exception ex)
            {
                throw new SpiceSharpException("Unhandled exception in SpiceSharpParser", ex);
            }

            result.Dependencies = new List<SpiceDependency>(_dependencies).AsReadOnly();
            return result;
        }

        private SpiceToken[] TokenizeWithRecovery(
            string source,
            string sourceName,
            SpiceLexerSettings lexerSettings)
        {
            string recoverySource = source;
            var recoveredLines = new HashSet<int>();

            while (true)
            {
                try
                {
                    SpiceToken[] tokens = TokenProviderPool
                        .GetSpiceTokenProvider(lexerSettings)
                        .GetTokens(recoverySource);
                    SetSourceName(tokens, sourceName);
                    return tokens;
                }
                catch (LexerException exception)
                {
                    if (!CanReportSyntaxError())
                    {
                        return null;
                    }

                    NormalizeLineInfo(exception.LineInfo, null, sourceName);
                    _activeValidation.AddError(
                        ValidationEntrySource.Lexer,
                        exception.Message,
                        exception.LineInfo,
                        exception);
                    _syntaxErrorCount++;

                    if (!CanRecover()
                        || !TryMaskSourceLine(ref recoverySource, exception.LineInfo?.LineNumber ?? 0, recoveredLines))
                    {
                        return null;
                    }
                }
            }
        }

        private SpiceNetlist ParseWithRecovery(
            SpiceToken[] tokens,
            string sourceName,
            ISingleSpiceNetlistParser parser)
        {
            SpiceToken[] recoveryTokens = tokens;
            var recoveredLines = new HashSet<int>();

            while (true)
            {
                try
                {
                    return parser.Parse(recoveryTokens);
                }
                catch (ParseException exception)
                {
                    if (!CanReportSyntaxError())
                    {
                        return null;
                    }

                    SpiceToken diagnosticToken = FindDiagnosticToken(recoveryTokens, exception.LineInfo?.LineNumber ?? 0);
                    NormalizeLineInfo(exception.LineInfo, diagnosticToken, sourceName);
                    _activeValidation.AddError(
                        ValidationEntrySource.Parser,
                        exception.Message,
                        exception.LineInfo,
                        exception);
                    _syntaxErrorCount++;

                    if (!CanRecover()
                        || IsGlobalParseError(exception)
                        || !TryRemoveStatementLine(
                            recoveryTokens,
                            exception.LineInfo?.LineNumber ?? 0,
                            recoveredLines,
                            out recoveryTokens))
                    {
                        return null;
                    }
                }
            }
        }

        private bool CanReportSyntaxError()
        {
            return _syntaxErrorCount < Settings.MaximumSyntaxErrors
                && (Settings.ContinueAfterErrors || _syntaxErrorCount == 0);
        }

        private bool CanRecover()
        {
            return Settings.ContinueAfterErrors && _syntaxErrorCount < Settings.MaximumSyntaxErrors;
        }

        private SpiceNetlist ParseExternalNetlist(
            string source,
            string sourceName,
            SpiceLexerSettings lexerSettings,
            SingleSpiceNetlistParserSettings parserSettings)
        {
            SpiceToken[] tokens = TokenizeWithRecovery(source, sourceName, lexerSettings);
            return tokens == null
                ? null
                : ParseWithRecovery(tokens, sourceName, new SingleSpiceNetlistParser(parserSettings));
        }

        private bool IsGlobalParseError(ParseException exception)
        {
            return exception.Message.StartsWith("No .END dot statement", StringComparison.Ordinal)
                || exception.Message.StartsWith("No new line at the end of the netlist", StringComparison.Ordinal)
                || exception.Message.StartsWith("End of tokens.", StringComparison.Ordinal)
                || exception.Message.StartsWith("Netlist ending - wrong ending", StringComparison.Ordinal);
        }

        private SpiceToken FindDiagnosticToken(SpiceToken[] tokens, int lineNumber)
        {
            return tokens.FirstOrDefault(token =>
                       token.LineNumber == lineNumber
                       && token.SpiceTokenType != SpiceTokenType.NEWLINE
                       && token.SpiceTokenType != SpiceTokenType.EOF)
                   ?? tokens.FirstOrDefault(token => token.LineNumber == lineNumber);
        }

        private void NormalizeLineInfo(SpiceLineInfo lineInfo, SpiceToken token, string sourceName)
        {
            if (lineInfo == null)
            {
                return;
            }

            if (token != null)
            {
                lineInfo.LineNumber = token.LineNumber;
                lineInfo.StartColumnIndex = token.StartColumnIndex;
                lineInfo.EndColumnIndex = Math.Max(token.EndColumnIndex, token.StartColumnIndex + 1);
                lineInfo.FileName = token.FileName;
            }
            else if (lineInfo.EndColumnIndex <= lineInfo.StartColumnIndex)
            {
                lineInfo.EndColumnIndex = lineInfo.StartColumnIndex + 1;
            }

            if (string.IsNullOrEmpty(lineInfo.FileName) && !string.IsNullOrEmpty(sourceName))
            {
                lineInfo.FileName = sourceName;
            }
        }

        private void SetSourceName(IEnumerable<SpiceToken> tokens, string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName))
            {
                return;
            }

            foreach (SpiceToken token in tokens)
            {
                token.FileName = sourceName;
            }
        }

        private bool TryMaskSourceLine(ref string source, int lineNumber, ISet<int> recoveredLines)
        {
            if (lineNumber <= 0 || !recoveredLines.Add(lineNumber))
            {
                return false;
            }

            int currentLine = 1;
            int start = 0;
            while (currentLine < lineNumber && start < source.Length)
            {
                if (source[start] == '\r')
                {
                    start += start + 1 < source.Length && source[start + 1] == '\n' ? 2 : 1;
                    currentLine++;
                }
                else if (source[start] == '\n')
                {
                    start++;
                    currentLine++;
                }
                else
                {
                    start++;
                }
            }

            if (currentLine != lineNumber || start >= source.Length)
            {
                return false;
            }

            int end = start;
            while (end < source.Length && source[end] != '\r' && source[end] != '\n')
            {
                end++;
            }

            if (IsStructuralSourceLine(source.Substring(start, end - start)))
            {
                return false;
            }

            char[] characters = source.ToCharArray();
            bool changed = false;
            for (int index = start; index < end; index++)
            {
                if (!char.IsWhiteSpace(characters[index]))
                {
                    characters[index] = ' ';
                    changed = true;
                }
            }

            if (!changed)
            {
                return false;
            }

            source = new string(characters);
            return true;
        }

        private bool IsStructuralSourceLine(string line)
        {
            string trimmed = line.TrimStart();
            string[] structuralKeywords =
            {
                ".end",
                ".ends",
                ".endl",
                ".endp",
                ".subckt",
                ".parallel",
                ".if",
                ".elseif",
                ".else",
                ".endif",
            };

            return structuralKeywords.Any(keyword =>
                trimmed.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)
                && (trimmed.Length == keyword.Length || char.IsWhiteSpace(trimmed[keyword.Length])));
        }

        private bool TryRemoveStatementLine(
            SpiceToken[] tokens,
            int lineNumber,
            ISet<int> recoveredLines,
            out SpiceToken[] recoveryTokens)
        {
            recoveryTokens = tokens;
            if (lineNumber <= 0 || !recoveredLines.Add(lineNumber))
            {
                return false;
            }

            SpiceToken[] lineTokens = tokens.Where(token => token.LineNumber == lineNumber).ToArray();
            if (lineTokens.Length == 0 || IsStructuralTokenLine(lineTokens))
            {
                return false;
            }

            recoveryTokens = tokens
                .Where(token =>
                    token.LineNumber != lineNumber
                    || token.SpiceTokenType == SpiceTokenType.NEWLINE
                    || token.SpiceTokenType == SpiceTokenType.EOF)
                .ToArray();

            return recoveryTokens.Length < tokens.Length;
        }

        private bool IsStructuralTokenLine(SpiceToken[] tokens)
        {
            if (tokens.Any(token =>
                token.SpiceTokenType == SpiceTokenType.END
                || token.SpiceTokenType == SpiceTokenType.ENDS
                || token.SpiceTokenType == SpiceTokenType.ENDL
                || token.SpiceTokenType == SpiceTokenType.ENDP
                || token.SpiceTokenType == SpiceTokenType.IF
                || token.SpiceTokenType == SpiceTokenType.ELSE
                || token.SpiceTokenType == SpiceTokenType.ELSE_IF
                || token.SpiceTokenType == SpiceTokenType.ENDIF
                || token.SpiceTokenType == SpiceTokenType.EOF))
            {
                return true;
            }

            for (int index = 0; index + 1 < tokens.Length; index++)
            {
                if (tokens[index].SpiceTokenType == SpiceTokenType.DOT
                    && (string.Equals(tokens[index + 1].Lexem, "SUBCKT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(tokens[index + 1].Lexem, "PARALLEL", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private void RecordDependency(SpiceDependency dependency)
        {
            _dependencies.Add(dependency);
        }

        private void SetSourceName(SpiceLineInfo lineInfo, string sourceName)
        {
            if (lineInfo != null && string.IsNullOrEmpty(lineInfo.FileName) && !string.IsNullOrEmpty(sourceName))
            {
                lineInfo.FileName = sourceName;
            }
        }

        private SpiceNetlist GetFinalModel(SpiceNetlist originalNetlistModel, ValidationEntryCollection validationResult)
        {
            SpiceNetlist finalModel = (SpiceNetlist)originalNetlistModel.Clone();
            var preprocessorContext = GetEvaluationContext();

            foreach (var preprocessor in Processors)
            {
                preprocessor.Validation = validationResult;

                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.EvaluationContext = preprocessorContext;
                    consumer.CaseSettings = Settings?.CaseSensitivity;
                }

                finalModel.Statements = preprocessor.Process(finalModel.Statements);
            }

            return finalModel;
        }

        private EvaluationContext GetEvaluationContext()
        {
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                Settings.CaseSensitivity.IsEntityNamesCaseSensitive,
                ".");

            var objectNameGenerator = new ObjectNameGenerator(string.Empty, ".");
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            var expressionParserFactory = new ExpressionParserFactory(Settings.CaseSensitivity, Settings.Compatibility);
            var expressionResolverFactory = new ExpressionResolverFactory(Settings.CaseSensitivity, Settings.Compatibility);

            EvaluationContext context = new SpiceEvaluationContext(
                string.Empty,
                Settings.CaseSensitivity,
                new Randomizer(
                    Settings.CaseSensitivity.IsDistributionNameCaseSensitive,
                    seed: 0),
                expressionParserFactory,
                new ExpressionFeaturesReader(expressionParserFactory, expressionResolverFactory),
                nameGenerator);

            context.Evaluator = new Evaluator(context, new ExpressionValueProvider(expressionParserFactory));

            return context;
        }
    }
}
