using SpiceSharpParser.ModelReader.Netlist.Spice.Context;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
{
    /// <summary>
    /// Interface for all model processors
    /// </summary>
    public interface IModelProcessor
    {
        /// <summary>
        /// Processes a model statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Process(SpiceSharpParser.Model.Netlist.Spice.Objects.Model statement, IProcessingContext context);
    }
}
