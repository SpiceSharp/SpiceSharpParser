using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Evaluation;
using SpiceSharpParser.Connector.Evaluation.CustomFunctions;
using SpiceSharpParser.Connector.Exceptions;
using SpiceSharpParser.Connector.Processors;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Connector
{
    /// <summary>
    /// Translates a netlist to Spice#.
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
        public SpiceSharpModel Translate(Model.Netlist netlist)
        {
            // Create result netlist
            var result = new SpiceSharpModel(new Circuit(), netlist.Title);

            // Create processing context
            var evaluator = new Evaluator();

            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" });
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);

            var processingContext = new ProcessingContext(
                string.Empty,
                evaluator,
                resultService,
                nodeNameGenerator,
                objectNameGenerator);

            AddSpiceFunctions(evaluator, processingContext);

            // Process statements form input netlist using created context
            StatementsProcessor.Process(netlist.Statements, processingContext);

            return result;
        }

        /// <summary>
        /// Adds user functions to evaluator
        /// </summary>
        /// <param name="evaluator">Evaluator to set</param>
        /// <param name="context">Processing context</param>
        private void AddSpiceFunctions(Evaluator evaluator, IProcessingContext context)
        {
            var customFunctions = new List<KeyValuePair<string, SpiceFunction>>();
            customFunctions.AddRange(ExportFunctions.Create(context, StatementsProcessor));
            customFunctions.AddRange(RandomFunctions.Create(context, StatementsProcessor));

            evaluator.ExpressionParser.CustomFunctions = customFunctions.ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }
}
