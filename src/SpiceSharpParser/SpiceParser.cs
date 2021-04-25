using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Expression;
using SpiceSharpParser.Parsers.Netlist;
using SpiceSharpParser.Parsers.Netlist.Spice;

namespace SpiceSharpParser
{
    /// <summary>
    /// The SPICE netlist parser.
    /// </summary>
    public class SpiceParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        public SpiceParser()
            : this(new SpiceParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        /// <param name="settings">SPICE parser settings.</param>
        public SpiceParser(SpiceParserSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SingleNetlistParser = new SingleSpiceNetlistParser(Settings.Parsing);

            TokenProviderPool = new SpiceTokenProviderPool();
            var includesPreprocessor = new IncludesPreprocessor(
                new FileReader(),
                TokenProviderPool,
                SingleNetlistParser,
                () => Settings.WorkingDirectory,
                Settings.Lexing);

            var libPreprocessor = new LibPreprocessor(
                new FileReader(),
                TokenProviderPool,
                SingleNetlistParser,
                includesPreprocessor,
                () => Settings.WorkingDirectory,
                Settings.Reading,
                Settings.Lexing);

            var appendModelPreprocessor = new AppendModelPreprocessor();
            var sweepsPreprocessor = new SweepsPreprocessor();
            var ifPostprocessor = new IfPreprocessor();
            var macroPreprocessor = new MacroPreprocessor();

            Preprocessors.AddRange(new IProcessor[] { includesPreprocessor, libPreprocessor, macroPreprocessor, appendModelPreprocessor, sweepsPreprocessor, ifPostprocessor });
        }

        public ISpiceTokenProviderPool TokenProviderPool { get; set; }

        /// <summary>
        /// Gets the parser parserSettings.
        /// </summary>
        public SpiceParserSettings Settings { get; }

        /// <summary>
        /// Gets the preprocessors.
        /// </summary>
        public List<IProcessor> Preprocessors { get; } = new List<IProcessor>();

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
        public SpiceParserResult ParseNetlist(string spiceNetlist)
        {
            if (spiceNetlist == null)
            {
                throw new ArgumentNullException(nameof(spiceNetlist));
            }

            var result = new SpiceParserResult { ValidationResult = new SpiceParserValidationResult() };

            // Get tokens
            try
            {
                var tokens = TokenProviderPool.GetSpiceTokenProvider(Settings.Lexing).GetTokens(spiceNetlist);

                SpiceNetlist originalNetlistModel = SingleNetlistParser.Parse(tokens);

                // Preprocessing
                var preprocessedNetListModel = GetPreprocessedNetListModel(originalNetlistModel, result.ValidationResult);

                // Reading model
                var reader = new SpiceNetlistReader(Settings.Reading);
                ISpiceModel<Circuit, Simulation> spiceModel = reader.Read(preprocessedNetListModel);

                result.OriginalInputModel = originalNetlistModel;
                result.PreprocessedInputModel = preprocessedNetListModel;
                result.SpiceModel = spiceModel;

                result.ValidationResult.Reading.AddRange(result.SpiceModel.ValidationResult);
            }
            catch (LexerException e)
            {
                result.ValidationResult.Lexing.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Lexer,
                        ValidationEntryLevel.Error,
                        e.ToString(),
                        null));
            }
            catch (ParseException e)
            {
                result.ValidationResult.Parsing.Add(
                    new ValidationEntry(
                        ValidationEntrySource.Parser,
                        ValidationEntryLevel.Error,
                        e.ToString(),
                        null));
            }

            return result;
        }

        private SpiceNetlist GetPreprocessedNetListModel(SpiceNetlist originalNetlistModel, SpiceParserValidationResult validationResult)
        {
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();
            var preprocessorContext = GetEvaluationContext();

            foreach (var preprocessor in Preprocessors)
            {
                preprocessor.Validation = validationResult;

                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.EvaluationContext = preprocessorContext;
                    consumer.CaseSettings = Settings.Reading?.CaseSensitivity;
                }

                preprocessedNetListModel.Statements = preprocessor.Process(preprocessedNetListModel.Statements);
            }

            return preprocessedNetListModel;
        }

        private EvaluationContext GetEvaluationContext()
        {
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                Settings.Reading.CaseSensitivity.IsEntityNamesCaseSensitive);

            var objectNameGenerator = new ObjectNameGenerator(string.Empty);
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            var expressionParserFactory = new ExpressionParserFactory(Settings.Reading.CaseSensitivity);

            EvaluationContext preprocessorContext = new SpiceEvaluationContext(
                string.Empty,
                Settings.Reading.EvaluatorMode,
                Settings.Reading.CaseSensitivity,
                new Randomizer(
                    Settings.Reading.CaseSensitivity.IsDistributionNameCaseSensitive,
                    seed: Settings.Reading.Seed),
                expressionParserFactory,
                new ExpressionFeaturesReader(expressionParserFactory),
                new ExpressionValueProvider(expressionParserFactory),
                nameGenerator,
                null);
            return preprocessorContext;
        }
    }
}