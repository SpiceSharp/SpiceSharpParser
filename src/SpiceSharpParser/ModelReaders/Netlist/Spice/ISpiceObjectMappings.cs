using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Registries;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    /// <summary>
    /// An interface for all SPICE object mappings.
    /// </summary>
    public interface ISpiceObjectMappings
    {
        /// <summary>
        /// Gets or sets control mapper.
        /// </summary>
        IMapper<BaseControl> Controls { get; set; }

        /// <summary>
        /// Gets or sets waveform mapper.
        /// </summary>
        IMapper<WaveformGenerator> Waveforms { get; set; }

        /// <summary>
        /// Gets or sets exporter mapper.
        /// </summary>
        IMapper<Exporter> Exporters { get; set; }

        /// <summary>
        /// Gets or sets components mapper.
        /// </summary>
        IMapper<EntityGenerator> Components { get; set; }

        /// <summary>
        /// Gets or sets models mapper.
        /// </summary>
        IMapper<ModelGenerator> Models { get; set; }
    }
}
