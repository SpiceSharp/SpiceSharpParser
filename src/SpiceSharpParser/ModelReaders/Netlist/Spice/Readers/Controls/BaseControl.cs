using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Common;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls
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
