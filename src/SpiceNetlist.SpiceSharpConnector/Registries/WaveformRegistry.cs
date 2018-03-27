using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;

namespace SpiceNetlist.SpiceSharpConnector.Registries
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
