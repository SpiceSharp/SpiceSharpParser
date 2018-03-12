using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Processes all <see cref="SubCircuit"/> from spice netlist object model.
    /// </summary>
    public class SubcircuitDefinitionProcessor : StatementProcessor<SubCircuit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinitionProcessor"/> class.
        /// </summary>
        public SubcircuitDefinitionProcessor()
        {
        }

        /// <summary>
        /// Processes a subcircuit statement
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A processing context</param>
        public override void Process(SubCircuit statement, ProcessingContext context)
        {
            context.AvailableSubcircuits.Add(statement);
        }
    }
}
