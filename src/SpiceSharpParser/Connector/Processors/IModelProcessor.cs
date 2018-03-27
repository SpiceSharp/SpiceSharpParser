using SpiceSharpParser.Connector.Context;

namespace SpiceSharpParser.Connector.Processors
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
        void Process(SpiceSharpParser.Model.SpiceObjects.Model statement, IProcessingContext context);
    }
}
