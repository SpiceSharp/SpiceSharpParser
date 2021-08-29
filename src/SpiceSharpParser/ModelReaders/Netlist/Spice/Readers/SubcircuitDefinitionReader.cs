using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads all <see cref="SubCircuit"/> from SPICE netlist object model.
    /// </summary>
    public class SubcircuitDefinitionReader : StatementReader<SubCircuit>, ISubcircuitDefinitionReader
    {
        /// <summary>
        /// Reads a subcircuit statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A reading context.</param>
        public override void Read(SubCircuit statement, IReadingContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.AvailableSubcircuits[statement.Name] = statement;
        }
    }
}