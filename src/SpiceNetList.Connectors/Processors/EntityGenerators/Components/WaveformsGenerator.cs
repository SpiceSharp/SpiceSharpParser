using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Components.Waveforms
{
    public class WaveformsGenerator
    {
        public Waveform Generate(ComplexParameter cp, NetList netlist)
        {
            if (cp.Name.ToLower() == "sine")
            {
                SineGenerator generator = new SineGenerator();
                return generator.Generate(cp, netlist);
            }
            if (cp.Name.ToLower() == "pulse")
            {
                PulseGenerator generator = new PulseGenerator();
                return generator.Generate(cp, netlist);
            }

            throw new System.Exception("Unsupported waveform");
        }
    }
}
