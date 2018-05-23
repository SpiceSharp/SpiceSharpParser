using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Processors
{
    /// <summary>
    /// Processes all <see cref="SubCircuit"/> from spice netlist object model.
    /// </summary>
    public class SubcircuitDefinitionProcessor : StatementProcessor<SubCircuit>, ISubcircuitDefinitionProcessor
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
        public override void Process(SubCircuit statement, IProcessingContext context)
        {
            context.AvailableSubcircuits.Add(statement);
        }
    }
}
