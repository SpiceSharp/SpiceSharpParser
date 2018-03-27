using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceSharpConnector.Context;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    /// <summary>
    /// Interface for all control processors
    /// </summary>
    public interface IControlProcessor
    {
        /// <summary>
        /// Processes a control statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modifify</param>
        void Process(Control statement, IProcessingContext context);
    }
}
