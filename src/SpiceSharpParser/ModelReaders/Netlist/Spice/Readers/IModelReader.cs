using SpiceSharpParser.ModelsReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all model readers
    /// </summary>
    public interface IModelReader
    {
        /// <summary>
        /// Reads a model statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Read(Models.Netlist.Spice.Objects.Model statement, IReadingContext context);
    }
}
