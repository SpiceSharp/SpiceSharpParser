using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    public class WaveformsGenerator
    {
        private PulseGenerator pulseGenerator;
        private SineGenerator sineGeneator;

        public WaveformsGenerator()
        {
            pulseGenerator = new PulseGenerator();
            sineGeneator = new SineGenerator(); 
        }
        public Waveform Generate(ComplexParameter cp, NetList netlist)
        {
            if (cp.Name.ToLower() == "sine")
            {
                return sineGeneator.Generate(cp, netlist);
            }
            if (cp.Name.ToLower() == "pulse")
            {
                return pulseGenerator.Generate(cp, netlist);
            }

            throw new System.Exception("Unsupported waveform");
        }
    }
}
