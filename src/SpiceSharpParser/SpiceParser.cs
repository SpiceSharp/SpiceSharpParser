using System.Collections.Generic;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice;
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
        /// <param name="spiceSingleNetlistParser">SPICE netlist parser.</param>
        /// <param name="preProcessors">Preprocessors.</param>
        public SpiceParser(
            ISingleSpiceNetlistParser spiceSingleNetlistParser,
            IProcessor[] preProcessors)
        {
            Settings = new SpiceParserSettings();
            SpiceSingleNetlistParser = spiceSingleNetlistParser ?? throw new System.ArgumentNullException(nameof(spiceSingleNetlistParser));

            if (preProcessors != null)
            {
                Preprocessors.AddRange(preProcessors);
            }
        }

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
        public SpiceParser(SpiceParserSettings settings)
        {
            Settings = settings;
            SpiceSingleNetlistParser = new SingleSpiceNetlistParser(Settings.Parsing);

            TokenProvider = new SpiceTokenProvider();
            var includesPreprocessor = new IncludesPreprocessor(
                new FileReader(),
                TokenProvider,
                SpiceSingleNetlistParser,
                () => Settings.WorkingDirectory,
                Settings.Reading);
            var libPreprocessor = new LibPreprocessor(new FileReader(), TokenProvider, SpiceSingleNetlistParser, includesPreprocessor, () => Settings.WorkingDirectory, Settings.Reading);
            var appendModelPreprocessor = new AppendModelPreprocessor();
            var sweepsPreprocessor = new SweepsPreprocessor();
            var ifPostprocessor = new IfPreprocessor();

            Preprocessors.AddRange(new IProcessor[] { includesPreprocessor, libPreprocessor, appendModelPreprocessor, sweepsPreprocessor, ifPostprocessor });
        }

        /// <summary>
        /// Gets or sets the token provider.
        /// </summary>
        public SpiceTokenProvider TokenProvider { get; }

        /// <summary>
        /// Gets or sets the parser settings.
        /// </summary>
        public SpiceParserSettings Settings { get; }

        /// <summary>
        /// Gets the pre processors.
        /// </summary>
        public List<IProcessor> Preprocessors { get; } = new List<IProcessor>();

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        protected ISingleSpiceNetlistParser SpiceSingleNetlistParser { get; }

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
                throw new System.ArgumentNullException(nameof(spiceNetlist));
            }

            if (Settings == null)
            {
                throw new System.InvalidOperationException(nameof(Settings));
            }

            // Get tokens
            var tokens = TokenProvider.GetTokens(spiceNetlist,
                Settings.Parsing.IsDotStatementCaseSensitive,
                Settings.Parsing.HasTitle,
                Settings.Parsing.IsEndRequired);

            SpiceNetlist originalNetlistModel = SpiceSingleNetlistParser.Parse(tokens);

            // Preprocessing
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();
            SpiceEvaluator preprocessorEvaluator = CreatePreprocessorEvaluator();

            EvaluatorsContainer evaluators = new EvaluatorsContainer(preprocessorEvaluator, new FunctionFactory());

            foreach (var preprocessor in Preprocessors)
            {
                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.Evaluators = evaluators;
                    consumer.CaseSettings = Settings.CaseSensitivity;
                }

                preprocessedNetListModel.Statements = preprocessor.Process(preprocessedNetListModel.Statements);
            }

            // Reading model
            var reader = new SpiceNetlistReader(Settings.Reading);
            SpiceNetlistReaderResult readerResult = reader.Read(preprocessedNetListModel);

            return new SpiceParserResult()
            {
                OriginalInputModel = originalNetlistModel,
                PreprocessedInputModel = preprocessedNetListModel,
                SpiceSharpModel = readerResult,
            };
        }

        private SpiceEvaluator CreatePreprocessorEvaluator()
        {
            SpiceEvaluator preprocessorEvaluator = new SpiceEvaluator(
                            "Preprocessors evaluator",
                            null,
                            Settings.Reading.EvaluatorMode,
                            Settings.Reading.Seed,
                            new ExpressionRegistry(Settings.CaseSensitivity.IsParameterNameCaseSensitive, Settings.CaseSensitivity.IsParameterNameCaseSensitive),
                            Settings.CaseSensitivity.IsFunctionNameCaseSensitive,
                            Settings.CaseSensitivity.IsParameterNameCaseSensitive);

            var exportFunctions = ExportFunctions.Create(
                Settings.Reading.Mappings.Exporters,
                new MainCircuitNodeNameGenerator(new string[] { "0" }, Settings.CaseSensitivity.IsNodeNameCaseSensitive),
                new ObjectNameGenerator(string.Empty),
                new ObjectNameGenerator(string.Empty),
                null,
                Settings.CaseSensitivity);

            foreach (var exportFunction in exportFunctions)
            {
                preprocessorEvaluator.Functions.Add(exportFunction.Key, exportFunction.Value);
            }

            return preprocessorEvaluator;
        }
    }
}
