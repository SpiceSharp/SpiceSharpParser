using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processes all <see cref="SubCircuit"/> from spice netlist object model.
    /// </summary>
    public class SubcircuitDefinitionProcessor : StatementProcessor<SubCircuit>
    {
        public SubcircuitDefinitionProcessor()
        {
        }

        public override void Process(SubCircuit statement, ProcessingContext context)
        {
            context.AvailableSubcircuits.Add(statement);
        }
    }
}
