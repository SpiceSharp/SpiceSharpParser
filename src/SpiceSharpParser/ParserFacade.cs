using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.ModelsReaders.Netlist.Spice;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Preprocessors;
using SpiceSharpParser.SpiceSharpParser.ModelsReaders.Netlist.Spice.Postprocessors;

namespace SpiceSharpParser
{
    /// <summary>
    /// A parser facade.
    /// </summary>
    public class ParserFacade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFacade"/> class.
        /// </summary>
        /// <param name="spiceNetlistParser">Spice netlist parser.</param>
        /// <param name="includesPreprocessor">Includes preprocessor.</param>
        /// <param name="appendModelPreprocessor">Append model preprocessor.</param>
        public ParserFacade(
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFacade"/> class.
        /// </summary>
        public ParserFacade()
        {
            SpiceNetlistParser = new SpiceNetlistParser();
            IncludesPreprocessor = new IncludesPreprocessor(new FileReader(), SpiceNetlistParser);
            AppendModelPreprocessor = new AppendModelPreprocessor();
            LibPreprocessor = new LibPreprocessor(new FileReader(), SpiceNetlistParser, IncludesPreprocessor);
            SweepsPreprocessor = new SweepsPreprocessor();
        }

        protected ISweepsPreprocessor SweepsPreprocessor { get; }

        /// <summary>
        /// Gets the .lib reader.
        /// </summary>
        protected ILibPreprocessor LibPreprocessor { get; }

        /// <summary>
        /// Gets the spice netlist parser.
        /// </summary>
        protected ISpiceNetlistParser SpiceNetlistParser { get; }

        /// <summary>
        /// Gets the includes reader.
        /// </summary>
        protected IIncludesPreprocessor IncludesPreprocessor { get; }

        /// <summary>
        /// Gets the appendmodel reader.
        /// </summary>
        protected IAppendModelPreprocessor AppendModelPreprocessor { get; }

        /// <summary>
        /// Parses the netlist.
        /// </summary>
        /// <param name="spiceNetlist">Netlist to parse.</param>
        /// <param name="settings">Setting for parser.</param>
        /// <returns>
        /// A parsing result.
        /// </returns>
        public ParserResult ParseNetlist(string spiceNetlist, ParserSettings settings)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            if (spiceNetlist == null)
            {
                throw new System.ArgumentNullException(nameof(spiceNetlist));
            }

            SpiceNetlist originalNetlistModel = SpiceNetlistParser.Parse(spiceNetlist, settings.SpiceNetlistParserSettings);
            SpiceNetlist preprocessedNetListModel = (SpiceNetlist)originalNetlistModel.Clone();

            // Preprocessing
            IncludesPreprocessor.Preprocess(preprocessedNetListModel, settings.WorkingDirectoryPath);
            LibPreprocessor.Preprocess(preprocessedNetListModel, settings.WorkingDirectoryPath);
            AppendModelPreprocessor.Preprocess(preprocessedNetListModel);
            SweepsPreprocessor.Preprocess(preprocessedNetListModel);
            // TODO: more preprocessors

            SpiceNetlist postprocessedNetlistModel = (SpiceNetlist)preprocessedNetListModel.Clone();

            // Postprocessing
            var postprocessorEvaluator = new SpiceEvaluator("Postprocessor evaluator", settings.SpiceNetlistModelReaderSettings.EvaluatorMode, new Common.Evaluation.ExpressionRegistry(), null);

            var ifPostprocessor = new IfPostprocessor(postprocessorEvaluator);
            postprocessedNetlistModel.Statements = ifPostprocessor.PostProcess(postprocessedNetlistModel.Statements);

            // Reading model
            var reader = new SpiceNetlistReader(settings.SpiceNetlistModelReaderSettings);
            SpiceNetlistReaderResult readerResult = reader.Read(postprocessedNetlistModel);

            return new ParserResult()
            {
                ReaderResult = readerResult,
                InitialNetlistModel = originalNetlistModel,
                PreprocessedNetlistModel = preprocessedNetListModel,
                PostprocessedNetlistModel = postprocessedNetlistModel,
            };
        }
    }
}
