using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;
using SpiceSharpParser.Model.Netlist.Spice.Objects;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Processors
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
