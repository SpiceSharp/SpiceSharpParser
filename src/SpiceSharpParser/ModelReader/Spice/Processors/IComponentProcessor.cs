using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Processors
{
    /// <summary>
    /// Interface for all component processors
    /// </summary>
    public interface IComponentProcessor
    {
        /// <summary>
        /// Processes a component statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Process(Component statement, IProcessingContext context);
    }
}
