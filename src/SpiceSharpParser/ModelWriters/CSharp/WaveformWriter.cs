using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public class WaveformWriter
    {
        private Dictionary<string, IWaveformWriter> Waveforms;

        public WaveformWriter()
        {
            Waveforms = new Dictionary<string, IWaveformWriter>(StringComparer.OrdinalIgnoreCase);

            // Register waveform generators
            Waveforms["SINE"] = new SineWriter();
            Waveforms["SIN"] = new SineWriter();
            Waveforms["PULSE"] = new PulseWriter();
            Waveforms["PWL"] = new PwlWriter();
            Waveforms["AM"] = new AMWriter();
            Waveforms["SFFM"] = new SFFMWriter();
            Waveforms["WAVE"] = new WaveWriter();
            Waveforms["wavefile"] = new WaveWriter();
        }

        public bool IsWaveFormSupported(string waveformName)
        {
            return Waveforms.ContainsKey(waveformName);
        }

        public List<CSharpStatement> GenerateWaveform(string name, ParameterCollection parameterCollection, out string waveFormId, IWriterContext context)
        {
            return Waveforms[name].Generate(parameterCollection, context, out waveFormId);
        }
    }
}
