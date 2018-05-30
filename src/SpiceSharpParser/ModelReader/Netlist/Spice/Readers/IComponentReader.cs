using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all component readers
    /// </summary>
    public interface IComponentReader
    {
        /// <summary>
        /// Reades a component statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Read(Component statement, IReadingContext context);
    }
}
