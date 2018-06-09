using SpiceSharp;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice
{
    /// <summary>
    /// Translates a spice model to Spice#.
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
            // Create result netlist
            var result = new SpiceNetlistReaderResult(new Circuit(), netlist.Title);

            // Create processing context
            var resultService = new ResultService(result);
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(new string[] { "0" });
            var objectNameGenerator = new ObjectNameGenerator(string.Empty);
            var mainEvaluator = new SpiceEvaluator(Settings.EvaluatorMode, Settings.Context.Exporters, nodeNameGenerator, objectNameGenerator);

            var readingContext = new ReadingContext(
                string.Empty,
                mainEvaluator,
                resultService,
                nodeNameGenerator,
                objectNameGenerator);

            // Read statements form input netlist using created context
            Settings.Context.Read(netlist.Statements, readingContext);

            return result;
        }
    }
}
