using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave
{
    public class WaveFileReadResult
    {
        public byte[] ChunkIdBytes { get; set; }

        public string ChunkId
        {
            get
            {
                return Encoding.ASCII.GetString(ChunkIdBytes);
            }
        }

        public int ChunkSize { get; set; }

        public int SampleRate { get; set; }

        public int NumberOfChannels { get; set; }

        public int BitsPerSample { get; set; }

        public short AudioFormat { get; set; }

        public byte[] FmtChunkIdBytes { get; set; }

        public string FmtChunkId
        {
            get
            {
                return Encoding.ASCII.GetString(FmtChunkIdBytes);
            }
        }

        public int FmtChunkSize { get; set; }

        public byte[] FormatBytes { get; set; }

        public string Format
        {
            get
            {
                return Encoding.ASCII.GetString(FormatBytes);
            }
        }

        public int ByteRate { get; set; }

        public short BlockAlign { get; set; }

        public byte[] DataChunkIdBytes { get; set; }

        public string DataChunkId
        {
            get
            {
                return Encoding.ASCII.GetString(DataChunkIdBytes);
            }
        }

        public int DataChunkSize { get; set; }

        public WaveFileChannelData[] ChannelData { get; set; }

        public (double, double)[] ConverToPwl(int channel, double amplitude)
        {
            if (NumberOfChannels <= channel)
            {
                return null;
            }

            var result = new List<(double, double)>();

            double step = 1.0 / SampleRate;

            for (var i = 0; i < ChannelData[channel].Data.Count; i++)
            {
                result.Add((step * i, (ChannelData[channel].Data[i] / (double)short.MaxValue) * amplitude));
            }

            return result.ToArray();
        }
    }
}
