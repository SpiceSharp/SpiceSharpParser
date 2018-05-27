using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    /// <summary>
    /// Base class for all statement processors
    /// </summary>
    /// <typeparam name="TStatement">A type of statement</typeparam>
    public abstract class StatementProcessor<TStatement> : IStatementProcessor
        where TStatement : Statement
    {
        /// <summary>
        /// Returns whether processor can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the processor can process given statement.
        /// </returns>
        public abstract bool CanProcess(Statement statement);

        /// <summary>
        /// Processes a statement (typed) and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public abstract void Process(TStatement statement, IProcessingContext context);

        /// <summary>
        /// Processes a statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        public void Process(Statement statement, IProcessingContext context)
        {
            this.Process((TStatement)statement, context);
        }
    }

    /// <summary>
    /// Base interface for all statement processors
    /// </summary>
    public interface IStatementProcessor
    {
        void Process(Statement statement, IProcessingContext context);

        /// <summary>
        /// Returns whether processor can process specific statement.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <returns>
        /// True if the processor can process given statement.
        /// </returns>
        bool CanProcess(Statement statement);
    }
}
