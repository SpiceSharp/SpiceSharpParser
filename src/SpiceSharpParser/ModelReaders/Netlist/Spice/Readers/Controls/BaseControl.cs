using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Base for all control readers.
    /// </summary>
    public abstract class BaseControl : StatementReader<Control>
    {
    }
}