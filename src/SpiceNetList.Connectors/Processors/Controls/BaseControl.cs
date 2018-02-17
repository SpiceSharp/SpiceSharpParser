using SpiceNetlist.SpiceObjects;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    public abstract class BaseControl : StatementProcessor<Control>
    {
        public abstract string Type
        {
            get;
        }
    }
}
