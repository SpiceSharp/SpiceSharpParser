using SpiceNetlist.SpiceSharpConnector.Processors.Controls;

namespace SpiceNetlist.SpiceSharpConnector
{
    /// <summary>
    /// Registry for <see cref="BaseControl"/>s
    /// </summary>
    public class ControlRegistry : BaseRegistry<BaseControl>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlRegistry"/> class.
        /// </summary>
        public ControlRegistry()
        {
        }
    }
}
