using SpiceSharpParser.ModelReaders.Netlist.Spice.Common;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Base for all control readers.
    /// </summary>
    public abstract class BaseControl : StatementReader<Control>, ISpiceObjectReader
    {
        /// <summary>
        /// Gets name of Spice dot command.
        /// </summary>
        public abstract string SpiceCommandName
        {
            get;
        }

        public override bool CanRead(Statement statement)
        {
            return statement is Control;
        }
    }
}
