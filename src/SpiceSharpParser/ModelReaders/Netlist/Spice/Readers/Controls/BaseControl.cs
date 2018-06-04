using SpiceSharpParser.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Common;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Base for all control readers.
    /// </summary>
    public abstract class BaseControl : StatementReader<Control>, ISpiceObjectReader
    {
        /// <summary>
        /// Gets name of Spice element.
        /// </summary>
        public abstract string SpiceName
        {
            get;
        }

        public override bool CanRead(Statement statement)
        {
            return statement is Control;
        }

        
    }
}
