using SpiceSharp;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    using SpiceSharpParser.Parsers.Expression;

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

            // Get result netlist
            var result = new SpiceNetlistReaderResult(
                new Circuit(StringComparerProvider.Get(Settings.CaseSensitivity.IsEntityNameCaseSensitive)),
                netlist.Title);

            // Get reading context
            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" }, Settings.CaseSensitivity.IsNodeNameCaseSensitive);
            var componentNameGenerator = new ObjectNameGenerator(string.Empty);
            var modelNameGenerator = new ObjectNameGenerator(string.Empty);

            IEvaluator readingEvaluator = CreateReadingEvaluator(nodeNameGenerator, componentNameGenerator, modelNameGenerator, resultService);


            ISimulationEvaluators simulationEvaluators = new SimulationEvaluators(readingEvaluator, new FunctionFactory());
            SimulationsParameters simulationParameters = new SimulationsParameters(simulationEvaluators);

            ISpiceStatementsReader statementsReader = new SpiceStatementsReader(Settings.Mappings.Controls, Settings.Mappings.Models, Settings.Mappings.Components);
            IWaveformReader waveformReader = new WaveformReader(Settings.Mappings.Waveforms);

            IExpressionParser parser = new ExpressionParserWithCache(new SpiceExpressionParser(Settings.EvaluatorMode == SpiceExpressionMode.LtSpice));


            var readingExpressionContext = CreateExpressionContext(
                nodeNameGenerator,
                componentNameGenerator,
                modelNameGenerator,
                resultService);

            var simulationContexts = new SimulationExpressionContexts(readingExpressionContext);

            IReadingContext readingContext = new ReadingContext(
                "Netlist reading context",
                parser,
                simulationParameters,
                simulationEvaluators,
                simulationContexts,
                resultService,
                nodeNameGenerator,
                componentNameGenerator,
                modelNameGenerator,
                statementsReader,
                waveformReader,
                readingEvaluator,
                readingExpressionContext,
                Settings.CaseSensitivity);

            // Read statements form input netlist using created context
            readingContext.Read(netlist.Statements, Settings.Orderer);

            UpdateSeed(readingContext, result);

            return result;
        }

        private void UpdateSeed(IReadingContext readingContext, SpiceNetlistReaderResult result)
        {
            int? seed = readingContext.Result.SimulationConfiguration.MonteCarloConfiguration.Seed
                        ?? readingContext.Result.SimulationConfiguration.Seed ?? this.Settings.Seed;

            readingContext.ReadingExpressionContext.Seed = seed;
            result.Seed = seed;
        }

        private ExpressionContext CreateExpressionContext(
            MainCircuitNodeNameGenerator nodeNameGenerator,
            ObjectNameGenerator componentNameGenerator,
            ObjectNameGenerator modelNameGenerator,
            IResultService result)
        {
            ExpressionContext rootContext = new SpiceExpressionContext(
                string.Empty,
                Settings.EvaluatorMode,
                Settings.CaseSensitivity.IsParameterNameCaseSensitive,
                Settings.CaseSensitivity.IsFunctionNameCaseSensitive,
                Settings.CaseSensitivity.IsExpressionNameCaseSensitive);

            var exportFunctions = ExportFunctions.Create(
                Settings.Mappings.Exporters,
                nodeNameGenerator,
                componentNameGenerator,
                modelNameGenerator,
                result,
                Settings.CaseSensitivity);

            foreach (var exportFunction in exportFunctions)
            {
                rootContext.Functions.Add(exportFunction.Key, exportFunction.Value);
            }

            return rootContext;
        }

        private SpiceEvaluator CreateReadingEvaluator(MainCircuitNodeNameGenerator nodeNameGenerator, ObjectNameGenerator componentNameGenerator, ObjectNameGenerator modelNameGenerator, IResultService result)
        {
            var readingEvaluator = new SpiceEvaluator(
                "Netlist reading evaluator",
                new ExpressionParserWithCache(
                    new SpiceExpressionParser(Settings.EvaluatorMode == SpiceExpressionMode.LtSpice)),
                Settings.CaseSensitivity.IsParameterNameCaseSensitive,
                Settings.CaseSensitivity.IsFunctionNameCaseSensitive);

            return readingEvaluator;
        }
    }
}
