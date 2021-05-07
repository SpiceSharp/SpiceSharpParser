using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Base class for all statement readers.
    /// </summary>
    /// <typeparam name="TStatement">A type of statement.</typeparam>
    public abstract class StatementReader<TStatement> : IStatementReader
        where TStatement : Statement
    {
        /// <summary>
        /// Reads a statement (typed) and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public abstract void Read(TStatement statement, IReadingContext context);

        /// <summary>
        /// Reads a statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public void Read(Statement statement, IReadingContext context)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Read((TStatement)statement, context);
        }
    }
}