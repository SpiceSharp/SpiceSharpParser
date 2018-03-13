using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Registries;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processes all <see cref="Statement"/> from spice netlist object model.
    /// </summary>
    public class StatementsProcessor : StatementProcessor<Statements>, IStatementsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementsProcessor"/> class.
        /// </summary>
        /// <param name="modelRegistry">A model registry</param>
        /// <param name="componentRegistry">A component registry</param>
        /// <param name="controlsRegistry">A controls registry</param>
        /// <param name="waveformsRegistry">A waveform registry</param>
        public StatementsProcessor(
            IEntityGeneratorRegistry modelRegistry,
            IEntityGeneratorRegistry componentRegistry,
            IControlRegistry controlsRegistry,
            IWaveformRegistry waveformsRegistry)
        {
            ModelProcessor = new ModelProcessor(modelRegistry);
            WaveformProcessor = new WaveformProcessor(waveformsRegistry);
            ControlProcessor = new ControlProcessor(controlsRegistry);

            SubcircuitDefinitionProcessor = new SubcircuitDefinitionProcessor();
            ComponentProcessor = new ComponentProcessor(ModelProcessor, componentRegistry);
            CommentProcessor = new CommentProcessor();
        }

        /// <summary>
        /// Gets the current model processor
        /// </summary>
        public ModelProcessor ModelProcessor { get; }

        /// <summary>
        /// Gets the current waveform processor
        /// </summary>
        public WaveformProcessor WaveformProcessor { get; }

        /// <summary>
        /// Gets the current component processor
        /// </summary>
        public ComponentProcessor ComponentProcessor { get; }

        /// <summary>
        /// Gets the current subcircuit processor
        /// </summary>
        public SubcircuitDefinitionProcessor SubcircuitDefinitionProcessor { get; }

        /// <summary>
        /// Gets the current control processor
        /// </summary>
        public ControlProcessor ControlProcessor { get; }

        /// <summary>
        /// Gets the current comment processor
        /// </summary>
        public CommentProcessor CommentProcessor { get; }

        /// <summary>
        /// Processes statemets and modifes the context
        /// </summary>
        /// <param name="statements">The statements to process</param>
        /// <param name="context">The context to modify</param>
        public override void Process(Statements statements, IProcessingContext context)
        {
            foreach (Statement statement in statements.OrderBy(StatementOrder))
            {
                var processor = GetProcessor(statement);
                if (processor != null)
                {
                    processor.Process(statement, context);
                }
            }
        }

        //TODO: refactor this
        private int StatementOrder(Statement statement)
        {
            if (statement is Model)
            {
                return 200;
            }

            if (statement is Component)
            {
                return 300;
            }

            if (statement is SubCircuit)
            {
                return 100;
            }

            if (statement is Control c)
            {
                return 0 + ControlProcessor.GetSubOrder(c);
            }

            if (statement is CommentLine)
            {
                return 0;
            }

            return -1;
        }

        // TODO: Refactor this
        private IStatementProcessor GetProcessor(Statement statement)
        {
            if (statement is Model)
            {
                return ModelProcessor;
            }

            if (statement is Component)
            {
                return ComponentProcessor;
            }

            if (statement is SubCircuit)
            {
                return SubcircuitDefinitionProcessor;
            }

            if (statement is Control)
            {
                return ControlProcessor;
            }

            if (statement is CommentLine)
            {
                return CommentProcessor;
            }

            throw new System.Exception("Unsupported statement");
        }
    }
}
