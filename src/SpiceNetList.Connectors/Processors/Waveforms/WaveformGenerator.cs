using SpiceNetlist.SpiceObjects.Parameters;
using SpiceNetlist.SpiceSharpConnector.Common;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.Waveforms
{
    public abstract class WaveformGenerator : ITyped
    {
        public abstract string Type { get; }

        public abstract Waveform Generate(BracketParameter bracketParameter, ProcessingContext context);
    }
}
