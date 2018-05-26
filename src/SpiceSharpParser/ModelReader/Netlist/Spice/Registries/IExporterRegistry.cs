using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls.Exporters;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Registries
{
    /// <summary>
    /// Interface for all exporter registries
    /// </summary>
    public interface IExporterRegistry
    {
        /// <summary>
        /// Adds an exporter to registy
        /// </summary>
        /// <param name="exporter">
        /// An exporter to add
        /// </param>
        void Add(Exporter exporter);

        /// <summary>
        /// Gets a value indicating whether a specified exporter is in registry
        /// </summary>
        /// <param name="type">Type of exporter</param>
        /// <returns>
        /// A value indicating whether a specified exporter is in registry
        /// </returns>
        bool Supports(string type);

        /// <summary>
        /// Gets the exporter by type
        /// </summary>
        /// <param name="type">Type of exporter</param>
        /// <returns>
        /// A reference to exporter
        /// </returns>
        Exporter Get(string type);

        IEnumerator<Exporter> GetEnumerator();
    }
}
