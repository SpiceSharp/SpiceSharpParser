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
            var result = new SpiceNetlistReaderResult(
                new Circuit(StringComparerFactory.Create(Settings.CaseSettings.IsEntityNameCaseSensitive)),
                netlist.Title);

            // Create reading context
            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" }, Settings.CaseSettings.IsNodeNameCaseSensitive);
            var componentNameGenerator = new ObjectNameGenerator(string.Empty);
            var modelNameGenerator = new ObjectNameGenerator(string.Empty);

            SpiceEvaluator readingEvaluator = CreateReadingEvaluator(nodeNameGenerator, componentNameGenerator, modelNameGenerator, resultService);
            var evaluatorsContainer = new EvaluatorsContainer(readingEvaluator, new FunctionFactory());
            var simulationParameters = new SimulationsParameters(evaluatorsContainer);

            var statementsReader = new SpiceStatementsReader(Settings.Mappings.Controls, Settings.Mappings.Models, Settings.Mappings.Components);
            var waveformReader = new WaveformReader(Settings.Mappings.Waveforms);

            var readingContext = new ReadingContext(
                "Netlist reading context",
                simulationParameters,
                evaluatorsContainer,
                resultService,
                nodeNameGenerator,
                componentNameGenerator,
                modelNameGenerator,
                statementsReader,
                waveformReader,
                Settings.CaseSettings);

            // Read statements form input netlist using created context
            readingContext.Read(netlist.Statements, Settings.Orderer);

            // Return and update evaluators info.
            result.Seed = result.Seed ?? Settings.Seed;

            result.Evaluators = evaluatorsContainer.GetEvaluators();

            return result;
        }

        private SpiceEvaluator CreateReadingEvaluator(MainCircuitNodeNameGenerator nodeNameGenerator, ObjectNameGenerator componentNameGenerator, ObjectNameGenerator modelNameGenerator, IResultService result)
        {
            var readingEvaluator = new SpiceEvaluator(
                            "Netlist reading evaluator",
                            null,
                            Settings.EvaluatorMode,
                            Settings.Seed,
                            new Common.Evaluation.ExpressionRegistry(Settings.CaseSettings.IsParameterNameCaseSensitive, Settings.CaseSettings.IsLetExpressionNameCaseSensitive),
                            Settings.CaseSettings.IsFunctionNameCaseSensitive,
                            Settings.CaseSettings.IsParameterNameCaseSensitive);

            var exportFunctions = ExportFunctions.Create(Settings.Mappings.Exporters, nodeNameGenerator, componentNameGenerator, modelNameGenerator, result, Settings.CaseSettings);
            foreach (var exportFunction in exportFunctions)
            {
                readingEvaluator.Functions.Add(exportFunction.Key, exportFunction.Value);
            }

            return readingEvaluator;
        }
    }
}
