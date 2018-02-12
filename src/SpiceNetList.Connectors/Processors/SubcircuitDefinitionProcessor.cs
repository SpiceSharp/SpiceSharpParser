using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class SubcircuitDefinitionProcessor : StatementProcessor
    {
        public SubcircuitDefinitionProcessor()
        {
        }

        public override void Init()
        {
        }

        public override void Process(Statement statement, ProcessingContext context)
        {
            var sub = statement as SubCircuit;
            context.AvailableDefinitions.Add(sub);
        }
    }
}
