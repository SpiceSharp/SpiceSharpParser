using System.Collections.Generic;
using SpiceNetlist.SpiceSharpConnector.Processors.Waveforms;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class WaveformRegistry
    {
        private List<WaveformGenerator> generators = new List<WaveformGenerator>();
        private Dictionary<string, WaveformGenerator> generatorsByType = new Dictionary<string, WaveformGenerator>();

        public WaveformRegistry()
        {
        }

        public void Add(WaveformGenerator control)
        {
            generators.Add(control);
            generatorsByType[control.Type] = control;
        }

        public bool Supports(string type)
        {
            return generatorsByType.ContainsKey(type);
        }

        public WaveformGenerator GetGenerator(string type)
        {
            return generatorsByType[type];
        }
    }
}
