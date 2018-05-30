using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all model readers
    /// </summary>
    public interface IModelReader
    {
        /// <summary>
        /// Reades a model statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Read(Model.Netlist.Spice.Objects.Model statement, IReadingContext context);
    }
}
