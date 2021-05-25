using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave
{
    public class WaveFileWriter
    {
        public void Write(string path, int sampleRate, double amplitude, int bitsPerSample, (double time, double value)[] leftChannel, (double time, double value)[] rightChannel)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (leftChannel == null)
            {
                throw new ArgumentNullException(nameof(leftChannel));
            }

            if (rightChannel == null)
            {
                throw new ArgumentNullException(nameof(rightChannel));
            }

            var result = new List<byte>();
            byte[] data = Compute(sampleRate, amplitude, bitsPerSample, leftChannel, rightChannel);

            WriteHeader(result, data, sampleRate, bitsPerSample, 2);
            WriteData(result, data);

            File.WriteAllBytes(path, result.ToArray());
        }

        public void Write(string path, int sampleRate, double amplitude, int bitsPerSample, (double time, double value)[] monoChannel)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (monoChannel == null)
            {
                throw new ArgumentNullException(nameof(monoChannel));
            }

            var result = new List<byte>();
            byte[] data = Compute(sampleRate, amplitude, bitsPerSample, monoChannel, null);
            WriteHeader(result, data, sampleRate, bitsPerSample, 1);
            WriteData(result, data);

            File.WriteAllBytes(path, result.ToArray());
        }

        /// <summary>
        /// Piece-wise linear interpolation.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="data">The interpolation data.</param>
        /// <returns>The interpolated value.</returns>
        public double Pwl(double arg, (double time, double value)[] data)
        {
            if (arg <= data[0].time)
            {
                return data[0].value;
            }

            if (arg >= data[data.Length - 1].time)
            {
                return data[data.Length - 1].value;
            }

            // Narrow in on the index for the piece-wise linear function
            int k0 = 0, k1 = data.Length;
            while (k1 - k0 > 1)
            {
                int k = (k0 + k1) / 2;
                if (data[k].time > arg)
                {
                    k1 = k;
                }
                else
                {
                    k0 = k;
                }
            }

            return data[k0].value + ((arg - data[k0].time) * (data[k1].value - data[k0].value) / (data[k1].time - data[k0].time));
        }

        private void WriteData(List<byte> result, byte[] data)
        {
            result.AddRange(data);
        }

        private void WriteHeader(List<byte> result, byte[] data, int sampleRate, int bitsPerSample, int numberOfChannels)
        {
            // header
            result.AddRange(Encoding.ASCII.GetBytes("RIFF")); //ChunkIdBytes

            var chunkSize = BitConverter.GetBytes(4 + (8 + 16) + (8 + data.Length));
            result.AddRange(chunkSize); //ChunkSize

            result.AddRange(Encoding.ASCII.GetBytes("WAVE")); //FormatBytes
            result.AddRange(Encoding.ASCII.GetBytes("fmt ")); //FmtChunkIdBytes

            var fmtChunkSize = BitConverter.GetBytes(16);
            result.AddRange(fmtChunkSize); //FmtChunkSize

            var audioFormat = BitConverter.GetBytes((short)1);
            result.AddRange(audioFormat); //AudioFormat

            var numberOfChannelsBytes = BitConverter.GetBytes((short)numberOfChannels);
            result.AddRange(numberOfChannelsBytes); //NumberOfChannels

            var sampleRateBytes = BitConverter.GetBytes(sampleRate);
            result.AddRange(sampleRateBytes); // SampleRate

            var byteRateBytes = BitConverter.GetBytes(sampleRate * numberOfChannels * bitsPerSample / 8);
            result.AddRange(byteRateBytes); // ByteRateBytes

            var byteAlignBytes = BitConverter.GetBytes((short)(numberOfChannels * bitsPerSample / 8));
            result.AddRange(byteAlignBytes); // ByteAlign

            var bitsPerSampleBytes = BitConverter.GetBytes((short)bitsPerSample);
            result.AddRange(bitsPerSampleBytes); //BitsPerSample

            result.AddRange(Encoding.ASCII.GetBytes("data"));
            result.AddRange(BitConverter.GetBytes(data.Length));
        }

        private byte[] Compute(int sampleRate, double amplitude, int bitsPerSample, (double time, double value)[] leftChannel, (double time, double value)[] rightChannel)
        {
            double step = 1.0 / sampleRate;

            List<byte> result = new List<byte>();

            var numberOfSamples = leftChannel[leftChannel.Length - 1].time / step;

            double currentTime = 0.0;

            if (bitsPerSample == 16)
            {
                for (var i = 0; i < numberOfSamples; i++)
                {
                    short leftChannelValue = (short)((Pwl(currentTime, leftChannel) / amplitude) * short.MaxValue);
                    result.AddRange(BitConverter.GetBytes(leftChannelValue));

                    if (rightChannel != null)
                    {
                        short rightChannelValue = (short)((Pwl(currentTime, rightChannel) / amplitude) * short.MaxValue);
                        result.AddRange(BitConverter.GetBytes(rightChannelValue));
                    }

                    currentTime += step;
                }
            }
            else if (bitsPerSample == 8)
            {
                for (var i = 0; i < numberOfSamples; i++)
                {
                    byte leftChannelValue = (byte)((Pwl(currentTime, leftChannel) / amplitude) * byte.MaxValue);
                    result.AddRange(BitConverter.GetBytes(leftChannelValue));

                    if (rightChannel != null)
                    {
                        byte rightChannelValue = (byte)((Pwl(currentTime, rightChannel) / amplitude) * byte.MaxValue);
                        result.AddRange(BitConverter.GetBytes(rightChannelValue));
                    }

                    currentTime += step;
                }
            }

            return result.ToArray();
        }
    }
}
