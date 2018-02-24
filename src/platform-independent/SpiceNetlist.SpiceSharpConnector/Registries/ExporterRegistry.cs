using System;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls.Exporters;

namespace SpiceNetlist.SpiceSharpConnector.Registries
{
    /// <summary>
    /// Registry for <see cref="Exporter"/>s
    /// </summary>
    public class ExporterRegistry : BaseRegistry<Exporter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterRegistry"/> class.
        /// </summary>
        public ExporterRegistry()
        {
        }

        /// <summary>
        /// Adds exporter to the registry (all generated types)
        /// </summary>
        /// <param name="exporter">
        /// A generator to add
        /// </param>
        public override void Add(Exporter exporter)
        {
            foreach (var type in exporter.GetSupportedTypes())
            {
                if (ElementsByType.ContainsKey(type))
                {
                    throw new Exception("Conflict in geneators");
                }

                ElementsByType[type] = exporter;
            }

            Elements.Add(exporter);
        }
    }
}
