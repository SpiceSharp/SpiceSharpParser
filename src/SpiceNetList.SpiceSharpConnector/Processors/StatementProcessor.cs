using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors;

namespace SpiceNetlist.SpiceSharpConnector
{
    public abstract class StatementProcessor<TStatement> : IStatementProcessor
        where TStatement : Statement
    {
        public abstract void Process(TStatement statement, ProcessingContext context);

        public void Process(Statement statement, ProcessingContext context)
        {
            this.Process((TStatement)statement, context);
        }
    }

    public interface IStatementProcessor
    {
        void Process(Statement statement, ProcessingContext context);
    }
}
