using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
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
        /// Returns whether processor can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the processor can process given statement.
        /// </returns>
        public override bool CanProcess(Statement statement)
        {
            return statement is SubCircuit;
        }

        /// <summary>
        /// Processes a subcircuit statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A processing context.</param>
        public override void Process(SubCircuit statement, IProcessingContext context)
        {
            context.AvailableSubcircuits.Add(statement);
        }
    }
}
