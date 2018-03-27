using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Processors;
using SpiceSharp;

namespace SpiceSharpParser.Connector
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
        public Netlist Translate(Model.Netlist netlist)
        {
            // Create result netlist
            var result = new Netlist(new Circuit(), netlist.Title);

            // Create processing context
            var evaluator = new Evaluator();
            var resultService = new ResultService(result);
            var nodeNameGenerator = new NodeNameGenerator();
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);

            var processingContext = new ProcessingContext(
                string.Empty,
                evaluator,
                resultService,
                nodeNameGenerator,
                objectNameGenerator);

            // Process statements form input netlist using created context
            StatementsProcessor.Process(netlist.Statements, processingContext);

            return result;
        }
    }
}
