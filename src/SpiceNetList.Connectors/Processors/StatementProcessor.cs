using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors;

namespace SpiceNetlist.SpiceSharpConnector
{
    public abstract class StatementProcessor
    {
        public abstract void Process(Statement statement, ProcessingContext context);

        public abstract void Init();
    }
}
