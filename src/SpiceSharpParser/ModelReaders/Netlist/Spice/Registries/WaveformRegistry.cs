using SpiceSharpParser.ModelsReaders.Netlist.Spice.Readers.Waveforms;

namespace SpiceSharpParser.ModelsReaders.Netlist.Spice.Registries
{
    /// <summary>
    /// Registry for <see cref="WaveformGenerator"/>s
    /// </summary>
    public class WaveformRegistry : BaseRegistry<WaveformGenerator>, IWaveformRegistry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveformRegistry"/> class.
        /// </summary>
        public WaveformRegistry()
        {
        }
    }
}
