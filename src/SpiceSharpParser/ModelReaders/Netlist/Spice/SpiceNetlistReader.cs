using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Parsers.Expression;

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
        /// <param name="settings">Netlist reader settings.</param>
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

            // Get result netlist
            var result = new SpiceNetlistReaderResult(
                new Circuit(StringComparerProvider.Get(Settings.CaseSensitivity.IsEntityNameCaseSensitive)),
                netlist.Title);

            IExpressionParser expressionParser = new ExpressionParser(Settings.CaseSensitivity);

            // Get reading context
            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" }, Settings.CaseSensitivity.IsNodeNameCaseSensitive);
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            IRandomizer randomizer = new Randomizer(
                Settings.CaseSensitivity.IsDistributionNameCaseSensitive,
                seed: Settings.Seed);

            var parsingEvaluationContext = CreateExpressionContext(expressionParser, nameGenerator, randomizer, resultService);
            var simulationEvaluationContexts = new SimulationEvaluationContexts(parsingEvaluationContext);
            ISimulationPreparations simulationPreparations = new SimulationPreparations(new EntityUpdates(Settings.CaseSensitivity.IsParameterNameCaseSensitive, simulationEvaluationContexts), new SimulationsUpdates(simulationEvaluationContexts));

            ICircuitEvaluator circuitEvaluator = new CircuitEvaluator(simulationEvaluationContexts, parsingEvaluationContext);
            ISpiceStatementsReader statementsReader = new SpiceStatementsReader(Settings.Mappings.Controls, Settings.Mappings.Models, Settings.Mappings.Components);
            IWaveformReader waveformReader = new WaveformReader(Settings.Mappings.Waveforms);

            ICircuitContext circuitContext = new CircuitContext(
                "Netlist reading context",
                null,
                circuitEvaluator,
                simulationPreparations,
                resultService,
                nameGenerator,
                statementsReader,
                waveformReader,
                Settings.CaseSensitivity,
                Settings.Mappings.Exporters,
                Settings.WorkingDirectory,
                null);

            // Set initial seed
            circuitContext.Evaluator.Seed = Settings.Seed;

            // Read statements form input netlist using created context
            circuitContext.Read(netlist.Statements, Settings.Orderer);

            // Set final seed
            result.Seed = circuitContext.Evaluator.Seed;

            return result;
        }

        private EvaluationContext CreateExpressionContext(IExpressionParser parser, INameGenerator nameGenerator, IRandomizer randomizer, IResultService resultService)
        {
            EvaluationContext rootContext = new SpiceEvaluationContext(
                string.Empty,
                Settings.EvaluatorMode,
                Settings.CaseSensitivity,
                randomizer,
                parser,
                nameGenerator,
                resultService);

            return rootContext;
        }
    }
}