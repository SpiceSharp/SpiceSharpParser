using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors
{
    public interface ISubcircuitDefinitionProcessor
    {
        /// <summary>
        /// Processes a subcircuit statement
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A processing context</param>
        void Process(SubCircuit statement, IProcessingContext context);
    }
}
