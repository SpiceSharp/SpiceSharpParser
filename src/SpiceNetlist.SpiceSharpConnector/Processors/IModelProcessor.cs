using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Context;

namespace SpiceNetlist.SpiceSharpConnector.Processors
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
        void Process(Model statement, IProcessingContext context);
    }
}
