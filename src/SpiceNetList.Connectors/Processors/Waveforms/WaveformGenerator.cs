using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class WaveformGenerator
    {
        private PulseGenerator pulseGenerator;
        private SineGenerator sineGeneator;

        public WaveformGenerator()
        {
            pulseGenerator = new PulseGenerator();
            sineGeneator = new SineGenerator();
        }

        public Waveform Generate(BracketParameter cp, ProcessingContext context)
        {
            if (cp.Name.ToLower() == "sine")
            {
                return sineGeneator.Generate(cp, context);
            }

            if (cp.Name.ToLower() == "pulse")
            {
                return pulseGenerator.Generate(cp, context);
            }

            throw new System.Exception("Unsupported waveform");
        }
    }
}
