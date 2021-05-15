using SpiceSharp.Components;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave
{
    public class Wave : Pwl
    {
        public Wave(byte[] fileContent, int channel, double amplitude = 1.0)
        {
            FileContent = fileContent ?? throw new System.ArgumentNullException(nameof(fileContent));
            Channel = channel;
            Amplitude = amplitude;

            var reader = new WaveFileReader(fileContent);
            var result = reader.Read();
            var pwlRawData = result.ConverToPwl(channel, amplitude);

            Points = pwlRawData.Select(raw => new Point(raw.Item1, raw.Item2));
        }

        public byte[] FileContent { get; }

        public int Channel { get; }

        public double Amplitude { get; }
    }
}
