using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Registries;
using SpiceSharpParser.Model.SpiceObjects;

namespace SpiceSharpParser.Connector.Processors
{
    public interface IStatementsProcessor
    {
        /// <summary>
        /// Gets exporter registry
        /// </summary>
        IExporterRegistry ExporterRegistry { get; }

        void Process(Statements statements, IProcessingContext context);
    }
}
