using System;
using System.IO;

namespace Drum_Machine.Services
{
    public static class WavReader
    {
        public static float[] ReadMono(string path)
        {
            using var reader = new BinaryReader(File.OpenRead(path));

            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF")
                throw new Exception("Invalid WAV file");

            reader.ReadInt32();

            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE")
                throw new Exception("Invalid WAV file");

            int channels = 1;
            int bitsPerSample = 16;
            short audioFormat = 1;
            byte[] data = null;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkId == "fmt ")
                {
                    audioFormat = reader.ReadInt16();
                    channels = reader.ReadInt16();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt16();
                    bitsPerSample = reader.ReadInt16();

                    if (chunkSize > 16)
                        reader.ReadBytes(chunkSize - 16);
                }
                else if (chunkId == "data")
                {
                    data = reader.ReadBytes(chunkSize);
                    break;
                }
                else
                {
                    reader.ReadBytes(chunkSize);
                }
            }

            if (data == null)
                throw new Exception("No data chunk found");

            int bytesPerSample = bitsPerSample / 8;
            int totalSamples = data.Length / bytesPerSample / channels;

            float[] result = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float sample = 0;

                for (int ch = 0; ch < channels; ch++)
                {
                    int offset = (i * channels + ch) * bytesPerSample;

                    float value;

                    switch (bitsPerSample)
                    {
                        case 16:
                            value = BitConverter.ToInt16(data, offset) / (float)short.MaxValue;
                            break;

                        case 24:
                            {
                                int sample24 = (data[offset + 2] << 16) |
                                               (data[offset + 1] << 8) |
                                               data[offset];

                                if ((sample24 & 0x800000) != 0)
                                    sample24 |= unchecked((int)0xFF000000);

                                value = sample24 / 8388608f;
                                break;
                            }

                        case 32:
                            {
                                if (audioFormat == 3)
                                    value = BitConverter.ToSingle(data, offset);
                                else
                                    value = BitConverter.ToInt32(data, offset) / (float)int.MaxValue;

                                break;
                            }

                        default:
                            throw new Exception("Unsupported bit depth");
                    }

                    sample += value;
                }

                result[i] = sample / channels;
            }

            return result;
        }
    }
}