using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public abstract class SingleControlProcessor
    {
        public abstract void Process(Control statement, ProcessingContext context);
    }
}
