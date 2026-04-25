using System;
using System.IO;
using System.Text;

namespace Drum_Machine.Services
{
    public interface IAudioExporter
    {
        void SaveWav(string path, float[] samples, int sampleRate);
    }

    public class WavExporter : IAudioExporter
    {
        public void SaveWav(string path, float[] samples, int sampleRate = 44100)
        {
            using var fs = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            int channels = 1;
            int bitsPerSample = 16;
            int bytesPerSample = bitsPerSample / 8;

            int byteRate = sampleRate * channels * bytesPerSample;
            int blockAlign = channels * bytesPerSample;
            int dataLength = samples.Length * bytesPerSample;

            float peak = 0f;
            foreach (var s in samples)
            {
                float abs = Math.Abs(s);
                if (abs > peak) peak = abs;
            }

            float gain = peak > 1f ? 1f / peak : 1f;

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);

            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataLength);

            foreach (var sample in samples)
            {
                float s = Math.Clamp(sample * gain, -1f, 1f);
                short val = (short)(s * 32767f);
                writer.Write(val);
            }
        }
    }
}