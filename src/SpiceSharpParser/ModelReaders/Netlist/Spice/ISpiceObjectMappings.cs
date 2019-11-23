using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;

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
        IMapper<IComponentGenerator> Components { get; set; }

        /// <summary>
        /// Gets or sets models mapper.
        /// </summary>
        IMapper<IModelGenerator> Models { get; set; }
    }
}