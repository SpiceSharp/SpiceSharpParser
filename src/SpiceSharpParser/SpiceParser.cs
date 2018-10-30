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
    using SpiceSharpParser.Parsers.Expression;

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
            SingleNetlistParser = spiceSingleNetlistParser ?? throw new System.ArgumentNullException(nameof(spiceSingleNetlistParser));

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
            SingleNetlistParser = new SingleSpiceNetlistParser(Settings.Parsing);

            TokenProvider = new SpiceTokenProvider();
            var includesPreprocessor = new IncludesPreprocessor(
                new FileReader(),
                TokenProvider,
                SingleNetlistParser,
                () => Settings.WorkingDirectory,
                Settings.Reading,
                Settings.Lexing);

            var libPreprocessor = new LibPreprocessor(
                new FileReader(),
                TokenProvider,
                SingleNetlistParser,
                includesPreprocessor,
                () => Settings.WorkingDirectory,
                Settings.Reading,
                Settings.Lexing);

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
        /// Gets or sets the parser parserSettings.
        /// </summary>
        public SpiceParserSettings Settings { get; }

        /// <summary>
        /// Gets the pre processors.
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
                throw new System.ArgumentNullException(nameof(spiceNetlist));
            }

            if (Settings == null)
            {
                throw new System.InvalidOperationException(nameof(Settings));
            }

            // Get tokens
            var tokens = TokenProvider.GetTokens(spiceNetlist, Settings.Lexing);

            SpiceNetlist originalNetlistModel = SingleNetlistParser.Parse(tokens);

            // Preprocessing
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();
            SpiceEvaluator preprocessorEvaluator = CreatePreprocessorEvaluator();

            ISimulationEvaluatorsContainer evaluators = new SimulationEvaluatorsContainer(preprocessorEvaluator, new FunctionFactory());

            foreach (var preprocessor in Preprocessors)
            {
                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.Evaluators = evaluators;
                    consumer.CaseSettings = Settings.Reading?.CaseSensitivity;
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
                            new SpiceExpressionParser(Settings.Reading.EvaluatorMode == SpiceEvaluatorMode.LtSpice),
                            Settings.Reading.EvaluatorMode,
                            Settings.Reading.Seed,
                            new ExpressionRegistry(
                                Settings.Reading.CaseSensitivity.IsParameterNameCaseSensitive, 
                                Settings.Reading.CaseSensitivity.IsParameterNameCaseSensitive),
                            Settings.Reading.CaseSensitivity.IsFunctionNameCaseSensitive,
                            Settings.Reading.CaseSensitivity.IsParameterNameCaseSensitive);

            var exportFunctions = ExportFunctions.Create(
                Settings.Reading.Mappings.Exporters,
                new MainCircuitNodeNameGenerator(new string[] { "0" }, Settings.Reading.CaseSensitivity.IsNodeNameCaseSensitive),
                new ObjectNameGenerator(string.Empty),
                new ObjectNameGenerator(string.Empty),
                null,
                Settings.Reading.CaseSensitivity);

            foreach (var exportFunction in exportFunctions)
            {
                preprocessorEvaluator.Functions.Add(exportFunction.Key, exportFunction.Value);
            }

            return preprocessorEvaluator;
        }
    }
}
