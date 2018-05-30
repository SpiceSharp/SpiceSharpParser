using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
{
    /// <summary>
    /// Base class for all statement readers
    /// </summary>
    /// <typeparam name="TStatement">A type of statement</typeparam>
    public abstract class StatementReader<TStatement> : IStatementReader
        where TStatement : Statement
    {
        /// <summary>
        /// Returns whether reader can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the reader can process given statement.
        /// </returns>
        public abstract bool CanRead(Statement statement);

        /// <summary>
        /// Reades a statement (typed) and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public abstract void Read(TStatement statement, IReadingContext context);

        /// <summary>
        /// Reades a statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public void Read(Statement statement, IReadingContext context)
        {
            this.Read((TStatement)statement, context);
        }
    }

    /// <summary>
    /// Base interface for all statement readers
    /// </summary>
    public interface IStatementReader
    {
        void Read(Statement statement, IReadingContext context);

        /// <summary>
        /// Returns whether reader can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the reader can process given statement.
        /// </returns>
        bool CanRead(Statement statement);
    }
}
