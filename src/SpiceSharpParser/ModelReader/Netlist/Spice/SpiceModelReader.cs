using SpiceSharp;
using SpiceSharpParser.Model.Netlist.Spice;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReader.Netlist.Spice.Evaluation.CustomFunctions;
using SpiceSharpParser.ModelReader.Netlist.Spice.Processors;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReader.Netlist.Spice
{
    /// <summary>
    /// Translates a spice model to Spice#.
    /// </summary>
    public class SpiceModelReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceModelReader"/> class.
        /// </summary>
        public SpiceModelReader(SpiceModelReaderSettings settings)
        {
            Settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
            StatementsProcessor = BuiltInProcessors.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceModelReader"/> class.
        /// </summary>
        /// <param name="statementsProcessor">Statements processor.</param>
        public SpiceModelReader(SpiceModelReaderSettings settings, IStatementsProcessor statementsProcessor)
        {
            Settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
            StatementsProcessor = statementsProcessor ?? throw new System.ArgumentNullException(nameof(statementsProcessor));
        }

        /// <summary>
        /// Gets the settings of the reader.
        /// </summary>
        public SpiceModelReaderSettings Settings { get; }

        /// <summary>
        /// Gets the statements processor.
        /// </summary>
        public IStatementsProcessor StatementsProcessor { get; private set; }

        /// <summary>
        /// Translates Netlist object mode to SpiceSharp netlist.
        /// </summary>
        /// <param name="netlist">A object model of the netlist.</param>
        /// <returns>
        /// A new SpiceSharp netlist.
        /// </returns>
        public SpiceModelReaderResult Read(SpiceNetlist netlist)
        {
            // Create result netlist
            var result = new SpiceModelReaderResult(new Circuit(), netlist.Title);

            // Create processing context
            var mainEvaluator = new SpiceEvaluator(Settings.EvaluatorMode);

            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" });
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);

            var processingContext = new ProcessingContext(
                string.Empty,
                mainEvaluator,
                resultService,
                nodeNameGenerator,
                objectNameGenerator);

            ExportFunctions.Add(mainEvaluator.CustomFunctions, processingContext, StatementsProcessor.GetRegistry<IExporterRegistry>());

            // Process statements form input netlist using created context
            StatementsProcessor.Process(netlist.Statements, processingContext);

            return result;
        }
    }
}
