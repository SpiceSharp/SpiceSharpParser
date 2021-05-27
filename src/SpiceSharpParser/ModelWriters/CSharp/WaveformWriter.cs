using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class WaveformWriter
    {
        private readonly Dictionary<string, IWaveformWriter> _waveforms;

        public WaveformWriter()
        {
            _waveforms = new Dictionary<string, IWaveformWriter>(StringComparer.OrdinalIgnoreCase)
            {
                ["SINE"] = new SineWriter(),
                ["SIN"] = new SineWriter(),
                ["PULSE"] = new PulseWriter(),
                ["PWL"] = new PwlWriter(),
                ["AM"] = new AMWriter(),
                ["SFFM"] = new SFFMWriter(),
                ["WAVE"] = new WaveWriter(),
                ["wavefile"] = new WaveWriter()
            };
        }

        public bool IsWaveFormSupported(string waveformName)
        {
            return _waveforms.ContainsKey(waveformName);
        }

        public List<CSharpStatement> GenerateWaveform(string name, ParameterCollection parameterCollection, out string waveFormId, IWriterContext context)
        {
            return _waveforms[name].Generate(parameterCollection, context, out waveFormId);
        }
    }
}
