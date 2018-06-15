using SpiceSharpParser.ModelsReaders.Netlist.Spice.Evaluation.CustomFunctions;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice
{
    public class SpiceNetlistReaderSettings
    {
        public SpiceNetlistReaderSettings()
        {
            EvaluatorMode = SpiceEvaluatorMode.Spice3f5;
            Context = new SpiceNetlistReaderContext();
        }

        /// <summary>
        /// Gets or sets the evaluator mode.
        /// </summary>
        public SpiceEvaluatorMode EvaluatorMode { get; set; }

        /// <summary>
        /// Gets or sets the context for reader.
        /// </summary>
        public ISpiceNetlistReaderContext Context { get; set; }

    }
}
