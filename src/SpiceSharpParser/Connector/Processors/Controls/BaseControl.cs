using SpiceSharpParser.Connector.Processors;
using SpiceSharpParser.Connector.Processors.Common;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors.Controls
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
