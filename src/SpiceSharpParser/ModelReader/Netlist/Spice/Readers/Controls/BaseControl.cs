using SpiceSharpParser.Common;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReader.Netlist.Spice.Common;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls
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
