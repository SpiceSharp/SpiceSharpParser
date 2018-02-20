using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class StatementsProcessor : StatementProcessor<Statements>
    {
        public StatementsProcessor(EntityGeneratorRegistry modelRegistry, EntityGeneratorRegistry componentRegistry, ControlRegistry controlsRegistry, WaveformRegistry waveformsRegistry)
        {
            ModelProcessor = new ModelProcessor(modelRegistry);
            WaveformProcessor = new WaveformProcessor(waveformsRegistry);
            ControlProcessor = new ControlProcessor(controlsRegistry);

            SubcircuitDefinitionProcessor = new SubcircuitDefinitionProcessor();
            ComponentProcessor = new ComponentProcessor(ModelProcessor, WaveformProcessor, componentRegistry);
            CommentProcessor = new CommentProcessor();
        }

        public ModelProcessor ModelProcessor { get; }

        public WaveformProcessor WaveformProcessor { get; }

        public ComponentProcessor ComponentProcessor { get; }

        public SubcircuitDefinitionProcessor SubcircuitDefinitionProcessor { get; }

        public ControlProcessor ControlProcessor { get; }

        public CommentProcessor CommentProcessor { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="context"></param>
        public override void Process(Statements statements, ProcessingContext context)
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
