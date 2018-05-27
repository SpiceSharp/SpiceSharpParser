using System.Collections.Generic;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    public interface IStatementsOrderer
    {
        /// <summary>
        /// Orders statements for processing.
        /// </summary>
        /// <param name="statements">Statement to order.</param>
        /// <returns>
        /// Ordered statements.
        /// </returns>
        IEnumerable<Statement> Order(Statements statements);
    }
}
