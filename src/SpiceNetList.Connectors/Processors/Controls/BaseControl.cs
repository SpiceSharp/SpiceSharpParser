using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Common;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    public abstract class BaseControl : StatementProcessor<Control>, ITyped
    {
        public abstract string Type
        {
            get;
        }
    }
}
