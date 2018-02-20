using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Processors.Common;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Controls
{
    /// <summary>
    /// Base for all control processors
    /// </summary>
    public abstract class BaseControl : StatementProcessor<Control>, IGenerator
    {
        /// <summary>
        /// Gets name of Spice element
        /// </summary>
        public abstract string TypeName
        {
            get;
        }
    }
}
