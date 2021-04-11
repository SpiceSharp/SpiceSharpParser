using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
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
        public SpiceModel<Circuit, Simulation> Read(SpiceNetlist netlist)
        {
            if (netlist == null)
            {
                throw new System.ArgumentNullException(nameof(netlist));
            }

            // Get result netlist
            var result = new SpiceModel<Circuit, Simulation>(
                new Circuit(new EntityCollection(StringComparerProvider.Get(Settings.CaseSensitivity.IsEntityNamesCaseSensitive))),
                netlist.Title);

            // Get reading context
            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                Settings.CaseSensitivity.IsEntityNamesCaseSensitive);
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            IRandomizer randomizer = new Randomizer(
                Settings.CaseSensitivity.IsDistributionNameCaseSensitive,
                seed: Settings.Seed);

            IExpressionParserFactory expressionParserFactory = new ExpressionParserFactory(Settings.CaseSensitivity);
            IExpressionFeaturesReader expressionFeaturesReader = new ExpressionFeaturesReader(expressionParserFactory);
            IExpressionValueProvider expressionValueProvider = new ExpressionValueProvider(expressionParserFactory);

            EvaluationContext expressionContext = new SpiceEvaluationContext(
                string.Empty,
                Settings.EvaluatorMode,
                Settings.CaseSensitivity,
                randomizer,
                expressionParserFactory,
                expressionFeaturesReader,
                expressionValueProvider,
                nameGenerator,
                resultService);

            var simulationEvaluationContexts = new SimulationEvaluationContexts(expressionContext);
            ISimulationPreparations simulationPreparations = new SimulationPreparations(
                new EntityUpdates(Settings.CaseSensitivity.IsParameterNameCaseSensitive, simulationEvaluationContexts),
                new SimulationsUpdates(simulationEvaluationContexts));

            ICircuitEvaluator circuitEvaluator = new CircuitEvaluator(simulationEvaluationContexts, expressionContext);
            ISpiceStatementsReader statementsReader = new SpiceStatementsReader(
                Settings.Mappings.Controls,
                Settings.Mappings.Models,
                Settings.Mappings.Components);
            IWaveformReader waveformReader = new WaveformReader(Settings.Mappings.Waveforms);

            ICircuitContext circuitContext = new CircuitContext(
                "Root circuit context",
                null,
                circuitEvaluator,
                simulationPreparations,
                resultService,
                nameGenerator,
                statementsReader,
                waveformReader,
                Settings.CaseSensitivity,
                Settings.Mappings.Exporters,
                Settings.WorkingDirectory);

            // Set initial seed
            circuitContext.Evaluator.Seed = Settings.Seed;

            // Read statements form input netlist using created context
            circuitContext.Read(netlist.Statements, Settings.Orderer);

            // Set final seed
            result.Seed = circuitContext.Evaluator.Seed;

            return result;
        }
    }
}