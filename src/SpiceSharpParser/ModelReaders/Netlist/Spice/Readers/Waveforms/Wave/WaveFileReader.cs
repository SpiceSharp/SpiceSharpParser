using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms.Wave
{
    public class WaveFileReader
    {
        private int _index;

        public WaveFileReader(byte[] fileContent, bool bigIndian = false)
        {
            FileContent = fileContent ?? throw new ArgumentNullException(nameof(fileContent));
            BigIndian = bigIndian;
        }

        public byte[] FileContent { get; }

        public bool BigIndian { get; }

        public WaveFileReadResult Read()
        {
            var result = new WaveFileReadResult();

            // header
            result.ChunkIdBytes = ReadBytes(4, BigIndian);
            result.ChunkSize = ReadInt32(BigIndian);
            result.FormatBytes = ReadBytes(4, BigIndian);
            result.FmtChunkIdBytes = ReadBytes(4, BigIndian);
            result.FmtChunkSize = ReadInt32(BigIndian);
            result.AudioFormat = ReadShort(BigIndian);
            result.NumberOfChannels = ReadShort(BigIndian);
            result.SampleRate = ReadInt32(BigIndian);
            result.ByteRate = ReadInt32(BigIndian);
            result.BlockAlign = ReadShort(BigIndian);
            result.BitsPerSample = ReadShort(BigIndian);
            result.DataChunkIdBytes = ReadBytes(4, BigIndian);
            result.DataChunkSize = ReadInt32(BigIndian);

            if (result.AudioFormat != 1)
            {
                throw new Exception("Only PCM audio format is supported");
            }

            if (result.NumberOfChannels != 1 && result.NumberOfChannels != 2)
            {
                throw new Exception("Only mono or stereo audio format is supported");
            }

            if (result.BitsPerSample != 8 && result.BitsPerSample != 16)
            {
                throw new Exception("Only 8 or 16 bits per sample is supported");
            }

            result.ChannelData = new WaveFileChannelData[result.NumberOfChannels];

            if (result.NumberOfChannels == 2)
            {
                result.ChannelData[0] = new WaveFileChannelData();
                result.ChannelData[1] = new WaveFileChannelData();
            }
            else
            {
                result.ChannelData[0] = new WaveFileChannelData();
            }

            while (_index < FileContent.Length)
            {
                int bytesPerSample = result.BitsPerSample / 8;

                if (result.NumberOfChannels == 1)
                {
                    var monoBytes = ReadBytes(bytesPerSample, BigIndian);

                    if (bytesPerSample == 1)
                    {
                        result.ChannelData[0].Data.Add((short)(255 * (monoBytes[0] - 127)));
                    }

                    if (bytesPerSample == 2)
                    {
                        result.ChannelData[0].Data.Add(BitConverter.ToInt16(monoBytes, 0));
                    }
                }
                else
                {
                    if (result.NumberOfChannels == 2)
                    {
                        var leftBytes = ReadBytes(bytesPerSample, BigIndian);
                        var rightBytes = ReadBytes(bytesPerSample, BigIndian);

                        if (leftBytes == null || rightBytes == null)
                        {
                            continue;
                        }

                        if (bytesPerSample == 1)
                        {
                            result.ChannelData[0].Data.Add((short)(255 * (leftBytes[0] - 127)));
                            result.ChannelData[1].Data.Add((short)(255 * (rightBytes[0] - 127)));
                        }

                        if (bytesPerSample == 2)
                        {
                            result.ChannelData[0].Data.Add(BitConverter.ToInt16(leftBytes, 0));
                            result.ChannelData[1].Data.Add(BitConverter.ToInt16(rightBytes, 0));
                        }
                    }
                }
            }

            return result;
        }

        public void Reset()
        {
            _index = 0;
        }

        public int ReadInt32(bool bigIndian)
        {
            var bytes = ReadBytes(4, bigIndian);

            int result = BitConverter.ToInt32(bytes, 0);

            return result;
        }

        public short ReadShort(bool bigIndian)
        {
            var bytes = ReadBytes(2, bigIndian);
            short result = BitConverter.ToInt16(bytes, 0);
            return result;
        }

        private byte[] ReadBytes(int count, bool bigIndian)
        {
            if (FileContent.Length <= _index)
            {
                return null;
            }

            byte[] result = new byte[count];

            Array.Copy(FileContent, _index, result, 0, count);

            _index += count;

            if (bigIndian)
            {
                Array.Reverse(result);
            }

            return result;
        }
    }
}
