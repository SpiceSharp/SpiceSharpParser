using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public interface IStatementsProcessor
    {
        void Process(Statements statements, ProcessingContextBase context);
    }
}
