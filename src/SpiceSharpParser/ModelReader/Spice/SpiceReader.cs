using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.ModelReader.Spice.Exceptions;
using SpiceSharpParser.ModelReader.Spice.Processors;
using SpiceSharpParser.Model.Spice;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReader.Spice
{
    /// <summary>
    /// Translates a netlist to Spice#.
    /// </summary>
    public class SpiceReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceReader"/> class.
        /// </summary>
        public SpiceReader()
        {
            StatementsProcessor = BuiltInProcessors.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceReader"/> class.
        /// </summary>
        /// <param name="statementsProcessor">Statements processor</param>
        public SpiceReader(IStatementsProcessor statementsProcessor)
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
        public SpiceReaderResult Read(Netlist netlist)
        {
            // Create result netlist
            var result = new SpiceReaderResult(new Circuit(), netlist.Title);

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
