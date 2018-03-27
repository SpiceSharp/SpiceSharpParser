using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors
{
    /// <summary>
    /// Base class for all statement processors
    /// </summary>
    /// <typeparam name="TStatement">A type of statement</typeparam>
    public abstract class StatementProcessor<TStatement> : IStatementProcessor
        where TStatement : Statement
    {
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
    }
}
