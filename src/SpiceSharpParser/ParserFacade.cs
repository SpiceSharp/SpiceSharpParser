using SpiceSharpParser.Model.Spice;
using SpiceSharpParser.ModelReader.Spice;
using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharpParser.Preprocessors;
using SpiceSharpParser.Postprocessors;
using SpiceSharpParser.Common;

namespace SpiceSharpParser
{
    /// <summary>
    /// SpiceSharpParser facade.
    /// </summary>
    public class ParserFacade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFacade"/> class.
        /// </summary>
        /// <param name="netlistModelReader">Netlist model reader.</param>
        /// <param name="includesProcessor">Includes preprocessor</param>
        /// <param name="appendModelProcessor">Append model preprocessor</param>
        public ParserFacade(
            INetlistModelReader netlistModelReader,
            IIncludesPreProcessor includesProcessor,
            IAppendModelPreProcessor appendModelProcessor,
            ILibPreProcessor libProcessor)
        {
            LibProcessor = libProcessor;
            IncludesProcessor = includesProcessor ?? throw new System.ArgumentNullException(nameof(includesProcessor));
            NetlistModelReader = netlistModelReader ?? throw new System.ArgumentNullException(nameof(netlistModelReader));
            AppendModelProcessor = appendModelProcessor ?? throw new System.ArgumentNullException(nameof(appendModelProcessor));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFacade"/> class.
        /// </summary>
        public ParserFacade()
        {
            NetlistModelReader = new NetlistModelReader();
            IncludesProcessor = new IncludesPreProcessor(new FileReader(), NetlistModelReader);
            AppendModelProcessor = new AppendModelPreProcessor();
            LibProcessor = new LibPreProcessor(new FileReader(), NetlistModelReader, IncludesProcessor);
        }

        /// <summary>
        /// Gets the .lib processor
        /// </summary>
        public ILibPreProcessor LibProcessor { get; }

        /// <summary>
        /// Gets the netlist model reader.
        /// </summary>
        public INetlistModelReader NetlistModelReader { get; }

        /// <summary>
        /// Gets the includes processor.
        /// </summary>
        public IIncludesPreProcessor IncludesProcessor { get; }

        /// <summary>
        /// Gets the appendmodel processor.
        /// </summary>
        public IAppendModelPreProcessor AppendModelProcessor { get; }

        /// <summary>
        /// Parses the netlist.
        /// </summary>
        /// <param name="netlist">Netlist to parse.</param>
        /// <param name="settings">Setting for parser.</param>
        /// <param name="workingDirectoryPath">A full path to working directory of the netlist.</param>
        /// <returns>
        /// A parsing result.
        /// </returns>
        public ParserResult ParseNetlist(string netlist, ParserSettings settings, string workingDirectoryPath = null)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            if (netlist == null)
            {
                throw new System.ArgumentNullException(nameof(netlist));
            }

            Netlist originalNetlistModel = NetlistModelReader.GetNetlistModel(netlist, settings);
            Netlist preprocessedNetListModel = (Netlist)originalNetlistModel.Clone();

            // Preprocessing
            IncludesProcessor.Process(preprocessedNetListModel, workingDirectoryPath);
            LibProcessor.Process(preprocessedNetListModel, workingDirectoryPath); 
            AppendModelProcessor.Process(preprocessedNetListModel);
            // TODO: more preprocessors

            Netlist postprocessedNetlistModel = (Netlist)preprocessedNetListModel.Clone();

            // Postprocessing
            var ifPostProcessor = new IfPostProcessor(new Evaluator());
            postprocessedNetlistModel.Statements = ifPostProcessor.Process(postprocessedNetlistModel.Statements);

            // Reading model
            var reader = new SpiceModelReader();
            SpiceModelReaderResult readerResult = reader.Read(postprocessedNetlistModel);

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
