using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public interface ISpiceStatementsOrderer
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