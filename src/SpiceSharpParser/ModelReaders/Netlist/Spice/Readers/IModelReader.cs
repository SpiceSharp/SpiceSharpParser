using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers
{
    /// <summary>
    /// Interface for all model readers.
    /// </summary>
    public interface IModelReader
    {
        /// <summary>
        /// Reads a model statement.
        /// </summary>
        /// <param name="statement">A statement to process,</param>
        /// <param name="context">A context.</param>
        void Read(Models.Netlist.Spice.Objects.Model statement, IReadingContext context);
    }
}