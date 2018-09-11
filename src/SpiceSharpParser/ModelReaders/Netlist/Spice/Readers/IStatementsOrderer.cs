using System.Collections.Generic;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    public interface IStatementsOrderer
    {
        /// <summary>
        /// Orders statements for reading.
        /// </summary>
        /// <param name="statements">Statement to order.</param>
        /// <returns>
        /// Ordered statements.
        /// </returns>
        IEnumerable<Statement> Order(Statements statements);
    }
}
