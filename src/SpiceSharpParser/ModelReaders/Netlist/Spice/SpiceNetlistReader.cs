using SpiceSharp;
using SpiceSharp.Entities;
using SpiceSharpBehavioral.Builders.Direct;
using SpiceSharpBehavioral.Builders.Functions;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Updates;
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
        public SpiceSharpModel Read(SpiceNetlist netlist)
        {
            if (netlist == null)
            {
                throw new System.ArgumentNullException(nameof(netlist));
            }

            // Get result netlist
            var result = new SpiceSharpModel(
                new Circuit(new EntityCollection(StringComparerProvider.Get(Settings.CaseSensitivity.IsEntityNamesCaseSensitive))),
                netlist.Title);

            // Set the separator.
            Utility.Separator = Settings.Separator;

            // Set functions case-sensitivity
            ComplexBuilderHelper.RemapFunctions(StringComparerProvider.Get(Settings.CaseSensitivity.IsFunctionNameCaseSensitive));
            ComplexFunctionBuilderHelper.RemapFunctions(StringComparerProvider.Get(Settings.CaseSensitivity.IsFunctionNameCaseSensitive));
            RealBuilderHelper.RemapFunctions(StringComparerProvider.Get(Settings.CaseSensitivity.IsFunctionNameCaseSensitive));
            RealFunctionBuilderHelper.RemapFunctions(StringComparerProvider.Get(Settings.CaseSensitivity.IsFunctionNameCaseSensitive));
            DerivativesHelper.RemapFunctions(StringComparerProvider.Get(Settings.CaseSensitivity.IsFunctionNameCaseSensitive));

            // Get reading context
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                Settings.CaseSensitivity.IsEntityNamesCaseSensitive,
                Settings.Separator);
            var objectNameGenerator = new ObjectNameGenerator(string.Empty, Settings.Separator);
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            IRandomizer randomizer = new Randomizer(
                Settings.CaseSensitivity.IsDistributionNameCaseSensitive,
                seed: Settings.Seed);

            IExpressionParserFactory expressionParserFactory = new ExpressionParserFactory(Settings.CaseSensitivity);
            IExpressionResolverFactory expressionResolverFactory = new ExpressionResolverFactory(Settings.CaseSensitivity);
            IExpressionFeaturesReader expressionFeaturesReader = new ExpressionFeaturesReader(expressionParserFactory, expressionResolverFactory);

            EvaluationContext evaluationContext = new SpiceEvaluationContext(
                string.Empty,
                Settings.CaseSensitivity,
                randomizer,
                expressionParserFactory,
                expressionFeaturesReader,
                nameGenerator);
            evaluationContext.Evaluator = new Evaluator(evaluationContext, new ExpressionValueProvider(expressionParserFactory));

            ISpiceStatementsReader statementsReader = new SpiceStatementsReader(
                Settings.Mappings.Controls,
                Settings.Mappings.Models,
                Settings.Mappings.Components);
            IWaveformReader waveformReader = new WaveformReader(Settings.Mappings.Waveforms);

            ISimulationPreparations simulationPreparations = new SimulationPreparations(
                new EntityUpdates(Settings.CaseSensitivity.IsParameterNameCaseSensitive, evaluationContext),
                new SimulationUpdates(evaluationContext.SimulationEvaluationContexts));

            IReadingContext circuitContext = new ReadingContext(
                "Root circuit context",
                null,
                evaluationContext,
                simulationPreparations,
                nameGenerator,
                statementsReader,
                waveformReader,
                Settings.Mappings.Exporters,
                new Readers.Controls.Simulations.Configurations.SimulationConfiguration(),
                result,
                Settings);

            // Read statements form input netlist using created context
            circuitContext.Read(netlist.Statements, Settings.Orderer);

            // Set final seed
            result.Seed = circuitContext.EvaluationContext.Seed;
            result.Circuit = circuitContext.ContextEntities;

            return result;
        }
    }
}