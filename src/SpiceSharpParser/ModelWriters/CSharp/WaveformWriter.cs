using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using SpiceSharpParser.ModelWriters.CSharp.Entities.Waveforms;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class WaveformWriter
    {
        private Dictionary<string, IWaveformWriter> _waveforms;

        public WaveformWriter()
        {
            _waveforms = new Dictionary<string, IWaveformWriter>(StringComparer.OrdinalIgnoreCase);

            // Register waveform generators
            _waveforms["SINE"] = new SineWriter();
            _waveforms["SIN"] = new SineWriter();
            _waveforms["PULSE"] = new PulseWriter();
            _waveforms["PWL"] = new PwlWriter();
            _waveforms["AM"] = new AMWriter();
            _waveforms["SFFM"] = new SFFMWriter();
            _waveforms["WAVE"] = new WaveWriter();
            _waveforms["wavefile"] = new WaveWriter();
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
