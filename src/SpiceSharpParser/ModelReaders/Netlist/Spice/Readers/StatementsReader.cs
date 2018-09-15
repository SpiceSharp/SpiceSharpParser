using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Reads <see cref="Statement"/>s from SPICE netlist object model.
    /// </summary>
    public class StatementsReader : IStatementsReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementsReader"/> class.
        /// </summary>
        public StatementsReader()
        {
        }

        /// <summary>
        /// Reads statemets and modifes the context.
        /// </summary>
        /// <param name="statements">The statements to process.</param>
        /// <param name="context">The context to modify.</param>
        public void Read(Statements statements, IReadingContext context, IStatementsOrderer orderer)
        {
            foreach (Statement statement in orderer.Order(statements))
            {
                context.Read(statement);
            }
        }
    }
}
