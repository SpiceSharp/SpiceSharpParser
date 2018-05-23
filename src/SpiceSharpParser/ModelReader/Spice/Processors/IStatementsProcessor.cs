using SpiceSharpParser.ModelReader.Spice.Context;
using SpiceSharpParser.ModelReader.Spice.Registries;
using SpiceSharpParser.Model.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Spice.Processors
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
