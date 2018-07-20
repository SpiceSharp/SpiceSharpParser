using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all <see cref="SubCircuit"/> from spice netlist object model.
    /// </summary>
    public class SubcircuitDefinitionReader : StatementReader<SubCircuit>, ISubcircuitDefinitionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitDefinitionReader"/> class.
        /// </summary>
        public SubcircuitDefinitionReader()
        {
        }

        /// <summary>
        /// Returns whether reader can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the reader can process given statement.
        /// </returns>
        public override bool CanRead(Statement statement)
        {
            return statement is SubCircuit;
        }

        /// <summary>
        /// Reads a subcircuit statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A reading context.</param>
        public override void Read(SubCircuit statement, IReadingContext context)
        {
            context.AvailableSubcircuits.Add(statement);
        }
    }
}
