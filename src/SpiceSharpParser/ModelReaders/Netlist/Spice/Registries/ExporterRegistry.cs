using System;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry of <see cref="Exporter"/>
    /// </summary>
    public class ExporterRegistry : BaseRegistry<Exporter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterRegistry"/> class.
        /// </summary>
        public ExporterRegistry()
        {
        }
    }
}
