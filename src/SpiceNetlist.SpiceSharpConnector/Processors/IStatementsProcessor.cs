using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Context;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public interface IStatementsProcessor
    {
        void Process(Statements statements, IProcessingContext context);
    }
}
