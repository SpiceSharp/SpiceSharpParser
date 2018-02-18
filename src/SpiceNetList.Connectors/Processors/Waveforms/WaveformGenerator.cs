using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Waveforms
{
    public abstract class WaveformGenerator
    {
        public abstract string Type { get; }

        public abstract Waveform Generate(BracketParameter bracketParameter, ProcessingContext context);
    }
}
