using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Postprocessors;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Preprocessors;
using SpiceSharpParser.Models.Netlist.Spice;

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
        /// <param name="includesPreprocessor">Includes preprocessor.</param>
        /// <param name="appendModelPreprocessor">Append model preprocessor.</param>
        public SpiceParser(
            ISpiceNetlistParser spiceNetlistParser,
            IIncludesPreprocessor includesPreprocessor,
            IAppendModelPreprocessor appendModelPreprocessor,
            ILibPreprocessor libPreprocessor,
            ISweepsPreprocessor sweepsPreprocessor)
        {
            LibPreprocessor = libPreprocessor ?? throw new System.ArgumentNullException(nameof(libPreprocessor));
            IncludesPreprocessor = includesPreprocessor ?? throw new System.ArgumentNullException(nameof(includesPreprocessor));
            SpiceNetlistParser = spiceNetlistParser ?? throw new System.ArgumentNullException(nameof(spiceNetlistParser));
            AppendModelPreprocessor = appendModelPreprocessor ?? throw new System.ArgumentNullException(nameof(appendModelPreprocessor));
            SweepsPreprocessor = sweepsPreprocessor ?? throw new System.ArgumentNullException(nameof(sweepsPreprocessor));
            Settings = new SpiceParserSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceParser"/> class.
        /// </summary>
        public SpiceParser()
        {
            SpiceNetlistParser = new SpiceNetlistParser();
            IncludesPreprocessor = new IncludesPreprocessor(new FileReader(), SpiceNetlistParser);
            AppendModelPreprocessor = new AppendModelPreprocessor();
            LibPreprocessor = new LibPreprocessor(new FileReader(), SpiceNetlistParser, IncludesPreprocessor);
            SweepsPreprocessor = new SweepsPreprocessor();
            Settings = new SpiceParserSettings();
        }

        /// <summary>
        /// Gets or sets the parser settings.
        /// </summary>
        public SpiceParserSettings Settings { get; set; }

        /// <summary>
        /// Gets the sweeps preprocessor.
        /// </summary>
        protected ISweepsPreprocessor SweepsPreprocessor { get; }

        /// <summary>
        /// Gets the .LIB preprocessor.
        /// </summary>
        protected ILibPreprocessor LibPreprocessor { get; }

        /// <summary>
        /// Gets the SPICE netlist parser.
        /// </summary>
        protected ISpiceNetlistParser SpiceNetlistParser { get; }

        /// <summary>
        /// Gets the includes preprocessor.
        /// </summary>
        protected IIncludesPreprocessor IncludesPreprocessor { get; }

        /// <summary>
        /// Gets the appendmodel preprocessor.
        /// </summary>
        protected IAppendModelPreprocessor AppendModelPreprocessor { get; }

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

            SpiceNetlist originalNetlistModel = SpiceNetlistParser.Parse(spiceNetlist, Settings.NetlistParser);
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();

            // Preprocessing
            IncludesPreprocessor.Preprocess(preprocessedNetListModel, Settings.WorkingDirectory);
            LibPreprocessor.Preprocess(preprocessedNetListModel, Settings.WorkingDirectory);
            AppendModelPreprocessor.Preprocess(preprocessedNetListModel);
            SweepsPreprocessor.Preprocess(preprocessedNetListModel);

            SpiceNetlist postprocessedNetlistModel = (SpiceNetlist)preprocessedNetListModel.Clone();

            // Postprocessing
            var postprocessorEvaluator = new SpiceEvaluator("Postprocessor evaluator", null, Settings.NetlistReader.EvaluatorMode, Settings.NetlistReader.Seed, new Common.Evaluation.ExpressionRegistry());

            var ifPostprocessor = new IfPostprocessor(postprocessorEvaluator);
            postprocessedNetlistModel.Statements = ifPostprocessor.PostProcess(postprocessedNetlistModel.Statements);

            // Reading model
            var reader = new SpiceNetlistReader(Settings.NetlistReader);
            SpiceNetlistReaderResult readerResult = reader.Read(postprocessedNetlistModel);

            return new SpiceParserResult()
            {
                Result = readerResult,
                InitialNetlistModel = originalNetlistModel,
                PreprocessedNetlistModel = preprocessedNetListModel,
                PostprocessedNetlistModel = postprocessedNetlistModel,
            };
        }
    }
}
