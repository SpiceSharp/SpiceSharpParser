using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class SubcircuitDefinitionProcessor : StatementProcessor<SubCircuit>
    {
        public SubcircuitDefinitionProcessor()
        {
        }

        public override void Process(SubCircuit statement, ProcessingContext context)
        {
            context.AvailableDefinitions.Add(statement);
        }
    }
}
