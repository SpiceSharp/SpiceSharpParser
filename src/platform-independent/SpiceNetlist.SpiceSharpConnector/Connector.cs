using SpiceNetlist.SpiceSharpConnector.Processors;
using SpiceSharp;

namespace SpiceNetlist.SpiceSharpConnector
{
    /// <summary>
    /// Translates a netlist to Spice#
    /// </summary>
    public class Connector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Connector"/> class.
        /// </summary>
        public Connector()
        {
            StatementsProcessor = BuiltInProcessors.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Connector"/> class.
        /// </summary>
        /// <param name="statementsProcessor">Statements processor</param>
        public Connector(IStatementsProcessor statementsProcessor)
        {
            StatementsProcessor = statementsProcessor ?? throw new System.ArgumentNullException(nameof(statementsProcessor));
        }

        /// <summary>
        /// Gets the statements processor
        /// </summary>
        public IStatementsProcessor StatementsProcessor { get; private set; }

        /// <summary>
        /// Translates Netlist object mode to SpiceSharp netlist
        /// </summary>
        /// <param name="netlist">A object model of the netlist</param>
        /// <returns>
        /// A new SpiceSharp netlist
        /// </returns>
        public Netlist Translate(SpiceNetlist.Netlist netlist)
        {
            // Create result netlist
            var result = new Netlist(new Circuit(), netlist.Title);

            // Create processing context
            var processingContext = new ProcessingContext(string.Empty, result);

            // Process statements form input netlist using created context
            StatementsProcessor.Process(netlist.Statements, processingContext);

            return result;
        }
    }
}
