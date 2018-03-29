namespace SpiceSharpParser
{
    /// <summary>
    /// A parser result
    /// </summary>
    public class ParserResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParserResult"/> class.
        /// </summary>
        public ParserResult()
        {
        }

        /// <summary>
        /// Gets or sets the connector result
        /// </summary>
        public Connector.ConnectorResult SpiceSharpModel { get; set; }

        /// <summary>
        /// Gets or sets the netlist model
        /// </summary>
        public Model.Netlist NetlistModel { get; set; }
    }
}
