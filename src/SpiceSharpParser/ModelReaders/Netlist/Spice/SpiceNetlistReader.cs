using SpiceSharp;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    /// <summary>
    /// Translates a SPICE model to SpiceSharp model.
    /// </summary>
    public class SpiceNetlistReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiceNetlistReader"/> class.
        /// </summary>
        public SpiceNetlistReader(SpiceNetlistReaderSettings settings)
        {
            Settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Gets the settings of the reader.
        /// </summary>
        public SpiceNetlistReaderSettings Settings { get; }

        /// <summary>
        /// Translates Netlist object mode to SpiceSharp netlist.
        /// </summary>
        /// <param name="netlist">A object model of the netlist.</param>
        /// <returns>
        /// A new SpiceSharp netlist.
        /// </returns>
        public SpiceNetlistReaderResult Read(SpiceNetlist netlist)
        {
            if (netlist == null)
            {
                throw new System.ArgumentNullException(nameof(netlist));
            }

            // Create result netlist
            var result = new SpiceNetlistReaderResult(new Circuit(), netlist.Title);

            // Create reading context
            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" });
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);
            var readingEvaluator = new SpiceEvaluator("Netlist reading evaluator", null, Settings.EvaluatorMode, Settings.Seed, Settings.Mappings.Exporters, nodeNameGenerator, objectNameGenerator);
            var simulationContexts = new SimulationContexts(resultService, readingEvaluator);
            var statementsReader = new SpiceStatementsReader(Settings.Mappings.Controls, Settings.Mappings.Models, Settings.Mappings.Components);
            var waveformReader = new WaveformReader(Settings.Mappings.Waveforms);

            var readingContext = new ReadingContext(
                string.Empty,
                simulationContexts,
                readingEvaluator,
                resultService,
                nodeNameGenerator,
                objectNameGenerator,
                statementsReader,
                waveformReader);

            // Read statements form input netlist using created context
            readingContext.Read(netlist.Statements, Settings.Orderer);

            // Return and update evaluators info.
            result.Seed = result.Seed ?? Settings.Seed;

            result.Evaluators = simulationContexts.GetSimulationEvaluators();

            return result;
        }
    }
}
