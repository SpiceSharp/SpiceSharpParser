using System;
using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Controls.Exporters;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry for <see cref="Exporter"/>s
    /// </summary>
    public class ExporterRegistry : BaseRegistry<Exporter>, IExporterRegistry
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
        /// <param name="element">
        /// A generator to add
        /// </param>
        public override void Add(Exporter element, bool canOverride = false)
        {
            foreach (var type in element.GetSupportedTypes())
            {
                if (ElementsByType.ContainsKey(type) && canOverride == false)
                {
                    throw new Exception("There is a generator for type: " + type);
                }

                ElementsByType[type] = element;
            }

            Elements.Add(element);
        }
    }
}
