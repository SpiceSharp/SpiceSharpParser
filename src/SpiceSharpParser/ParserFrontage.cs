namespace SpiceSharpParser
{
    /// <summary>
    /// SpiceSharpParser front
    /// </summary>
    public class ParserFrontage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFrontage"/> class.
        /// </summary>
        /// <param name="netlistModelReader">Netlist model reader</param>
        public ParserFrontage(INetlistModelReader netlistModelReader)
        {
            NetlistModelReader = netlistModelReader ?? throw new System.ArgumentNullException(nameof(netlistModelReader));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserFrontage"/> class.
        /// </summary>
        public ParserFrontage()
        {
            NetlistModelReader = new NetlistModelReader();
        }

        /// <summary>
        /// Gets the netlist model reader
        /// </summary>
        public INetlistModelReader NetlistModelReader { get; }

        /// <summary>
        /// Parses the netlist
        /// </summary>
        /// <param name="netlist">Netlist to parse</param>
        /// <param name="settings">Setting for parser</param>
        /// <returns>
        /// A parsing result
        /// </returns>
        public ParserResult ParseNetlist(string netlist, ParserSettings settings)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            if (netlist == null)
            {
                throw new System.ArgumentNullException(nameof(netlist));
            }

            Model.Netlist netlistModel = NetlistModelReader.GetNetlistModel(netlist, settings);

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
        private Connector.ConnectorResult GetConnectorResult(SpiceSharpParser.Model.Netlist netlist)
        {
            var connector = new Connector.Connector();
            return connector.Translate(netlist);
        }
    }
}
