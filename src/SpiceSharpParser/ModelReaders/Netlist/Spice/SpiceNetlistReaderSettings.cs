using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceNetlistReaderSettings
    {
        public SpiceNetlistReaderSettings()
        {
            EvaluatorMode = SpiceEvaluatorMode.Spice3f5;
            Entities = new SpiceEntityRegistry();
            Orderer = new StatementsOrderer();
        }

        /// <summary>
        /// Gets or sets the evaluator mode.
        /// </summary>
        public SpiceEvaluatorMode EvaluatorMode { get; set; }

        /// <summary>
        /// Gets or sets the evaluator random seed.
        /// </summary>
        public int? Seed { get; set; }

        /// <summary>
        /// Gets or sets the entity registry.
        /// </summary>
        public ISpiceEntityRegistry Entities { get; set; }

        /// <summary>
        /// Gets or sets the statements orderer.
        /// </summary>
        public IStatementsOrderer Orderer { get; set; }
    }
}
