using SpiceSharpParser.Model;

namespace SpiceSharpParser
{
    /// <summary>
    /// SpiceSharpParser facade
    /// </summary>
    public class ParserFacade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFacade"/> class.
        /// </summary>
        /// <param name="netlistModelReader">Netlist model reader</param>
        public ParserFacade(INetlistModelReader netlistModelReader, IIncludesProcessor includesProcessor)
        {
            IncludesProcessor = includesProcessor ?? throw new System.ArgumentNullException(nameof(includesProcessor));
            NetlistModelReader = netlistModelReader ?? throw new System.ArgumentNullException(nameof(netlistModelReader));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFacade"/> class.
        /// </summary>
        public ParserFacade()
        {
            NetlistModelReader = new NetlistModelReader();
            IncludesProcessor = new IncludesProcessor(new FileReader(), NetlistModelReader);
        }

        /// <summary>
        /// Gets the netlist model reader
        /// </summary>
        public INetlistModelReader NetlistModelReader { get; }

        /// <summary>
        /// Gets the includes processor
        /// </summary>
        public IIncludesProcessor IncludesProcessor { get; }

        /// <summary>
        /// Parses the netlist
        /// </summary>
        /// <param name="netlist">Netlist to parse</param>
        /// <param name="settings">Setting for parser</param>
        /// <param name="workingDirectoryPath">A full path to working directory of the netlist</param>
        /// <returns>
        /// A parsing result
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

            Netlist netlistModel = NetlistModelReader.GetNetlistModel(netlist, settings);

            IncludesProcessor.Process(netlistModel, workingDirectoryPath);

            Connector.ConnectorResult connectorResult = GetConnectorResult(netlistModel);

            return new ParserResult()
            {
                SpiceSharpModel = connectorResult,
                NetlistModel = netlistModel
            };
        }

        /// <summary>
        /// Gets the SpiceSharp model for the netlist
        /// </summary>
        /// <param name="netlist">Netlist model</param>
        /// <returns>
        /// A new SpiceSharp model for the netlist
        /// </returns>
        private Connector.ConnectorResult GetConnectorResult(Netlist netlist)
        {
            var connector = new Connector.Connector();
            return connector.Translate(netlist);
        }
    }
}
