using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Processors
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
