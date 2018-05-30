using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all control readers
    /// </summary>
    public interface IControlReader
    {
        /// <summary>
        /// Reades a control statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Read(Control statement, IReadingContext context);
    }
}
