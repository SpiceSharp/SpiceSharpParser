using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .GLOBAL <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class GlobalControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, IReadingContext context)
        {
            foreach (var param in statement.Parameters)
            {
                if ((param is Models.Netlist.Spice.Objects.Parameters.SingleParameter sp)
                    && (sp is Models.Netlist.Spice.Objects.Parameters.WordParameter
                      || sp is Models.Netlist.Spice.Objects.Parameters.IdentifierParameter
                      || sp is Models.Netlist.Spice.Objects.Parameters.ValueParameter))
                {
                    context.NameGenerator.SetGlobal(sp.Value);
                }
            }
        }
    }
}