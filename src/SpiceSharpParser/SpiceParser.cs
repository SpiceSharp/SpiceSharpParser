using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Processors;
using SpiceSharpParser.Models.Netlist.Spice;
using System.Collections.Generic;

namespace SpiceSharpParser
{
    /// <summary>
    /// The SPICE parser.
    /// </summary>
    public class SpiceParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        /// <param name="spiceNetlistParser">SPICE netlist parser.</param>
        /// <param name="preProcessors">Preprocessors.</param>
        public SpiceParser(
            ISpiceNetlistParser spiceNetlistParser,
            IProcessor[] preProcessors)
        {
            Settings = new SpiceParserSettings();
            SpiceNetlistParser = spiceNetlistParser ?? throw new System.ArgumentNullException(nameof(spiceNetlistParser));

            if (preProcessors != null)
            {
                Preprocessors.AddRange(preProcessors);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        public SpiceParser()
        {
            Settings = new SpiceParserSettings();
            SpiceNetlistParser = new SpiceNetlistParser();

            var includesPreprocessor = new IncludesPreprocessor(new FileReader(), SpiceNetlistParser);
            var appendModelPreprocessor = new AppendModelPreprocessor();
            var libPreprocessor = new LibPreprocessor(new FileReader(), SpiceNetlistParser, includesPreprocessor);
            var sweepsPreprocessor = new SweepsPreprocessor();
            var ifPostprocessor = new IfPreprocessor();

            Preprocessors.AddRange(new IProcessor[] { includesPreprocessor, libPreprocessor, appendModelPreprocessor, sweepsPreprocessor, ifPostprocessor });
        }

        /// <summary>
        /// Gets or sets the parser settings.
        /// </summary>
        public SpiceParserSettings Settings { get; set; }

        /// <summary>
        /// Gets the pre processors.
        /// </summary>
        public List<IProcessor> Preprocessors { get; } = new List<IProcessor>();

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        protected ISpiceNetlistParser SpiceNetlistParser { get; }
      
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

            SpiceNetlist originalNetlistModel = SpiceNetlistParser.Parse(spiceNetlist, Settings.Parsing);

            // Preprocessing
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();
            SpiceEvaluator preprocessorEvaluator = new SpiceEvaluator(
                "Preprocessors evaluator",
                null,
                Settings.Reading.EvaluatorMode,
                Settings.Reading.Seed,
                Settings.Reading.Mappings.Exporters,
                new MainCircuitNodeNameGenerator(new string[] { "0" }),
                new ObjectNameGenerator(string.Empty));

            foreach (var preprocessor in Preprocessors)
            {
                if (preprocessor is IEvaluatorConsumer consumer)
                {
                    consumer.Evaluator = preprocessorEvaluator;
                }

                preprocessedNetListModel.Statements = preprocessor.Process(preprocessedNetListModel.Statements);
            }

            // Reading model
            var reader = new SpiceNetlistReader(Settings.Reading);
            SpiceNetlistReaderResult readerResult = reader.Read(preprocessedNetListModel);

            return new SpiceParserResult()
            {
                InitialNetlistModel = originalNetlistModel,
                PreprocessedNetlistModel = preprocessedNetListModel,
                Result = readerResult,
            };
        }
    }
}
