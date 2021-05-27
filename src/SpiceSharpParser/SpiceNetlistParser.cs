using System;
using System.Collections.Generic;
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
            if (spiceNetlist == null)
            {
                throw new ArgumentNullException(nameof(spiceNetlist));
            }

            var result = new SpiceNetlistParseResult { ValidationResult = new ValidationEntryCollection() };

            // Get tokens
            try
            {
                var tokens = TokenProviderPool.GetSpiceTokenProvider(Settings.Lexing).GetTokens(spiceNetlist);

                SpiceNetlist originalNetlistModel = SingleNetlistParser.Parse(tokens);

                result.InputModel = originalNetlistModel;
                result.FinalModel = GetFinalModel(originalNetlistModel, result.ValidationResult);
            }
            catch (LexerException e)
            {
                result.ValidationResult.AddError(
                    ValidationEntrySource.Lexer,
                    "General error during lexing",
                    e.LineInfo,
                    e);
            }
            catch (ParseException e)
            {
                result.ValidationResult.AddError(
                    ValidationEntrySource.Parser,
                    "General error during parsing",
                    e.LineInfo,
                    e);
            }
            catch (Exception ex)
            {
                throw new SpiceSharpException("Unhandled exception in SpiceSharpParser", ex);
            }

            return result;
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
            var expressionParserFactory = new ExpressionParserFactory(Settings.CaseSensitivity);
            var expressionResolverFactory = new ExpressionResolverFactory(Settings.CaseSensitivity);

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