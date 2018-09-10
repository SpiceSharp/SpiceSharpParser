using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry for <see cref="BaseControl"/>s
    /// </summary>
    public class ControlRegistry : BaseRegistry<BaseControl>, IControlRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlRegistry"/> class.
        /// </summary>
        public ControlRegistry()
        {
        }
    }
}
