using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry of <see cref="BaseControl"/>
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
