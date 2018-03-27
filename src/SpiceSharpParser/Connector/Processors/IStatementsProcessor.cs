using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors
{
    public interface IStatementsProcessor
    {
        void Process(Statements statements, IProcessingContext context);
    }
}
