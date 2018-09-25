using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all <see cref="SubCircuit"/> from SPICE netlist object model.
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
