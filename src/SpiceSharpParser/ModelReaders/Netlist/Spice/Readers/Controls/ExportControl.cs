using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    public abstract class ExportControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportControl"/> class.
        /// </summary>
        /// <param name="mapper">The exporter mapper.</param>
        public ExportControl(IMapper<Exporter> mapper, IExportFactory exportFactory)
        {
            Mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
            ExportFactory = exportFactory ?? throw new System.ArgumentNullException(nameof(exportFactory));
        }

        /// <summary>
        /// Gets the exporter mapper.
        /// </summary>
        protected IMapper<Exporter> Mapper { get; }

        /// <summary>
        /// Gets the export factory.
        /// </summary>
        protected IExportFactory ExportFactory { get; }

        /// <summary>
        /// Generates a new export.
        /// </summary>
        protected Export GenerateExport(Parameter parameter, IReadingContext context, Simulation simulation)
        {
            return ExportFactory.Create(parameter, context, simulation, Mapper);
        }
    }
}
