using System.IO;
using System.Collections.Generic;
using Drum_Machine.Models;
using Drum_Machine.Services;

namespace Drum_Machine.Core
{
    public class DrumMachine
    {
        public List<BaseTrack> Tracks { get; set; } = new List<BaseTrack>();
        public int CurrentStep { get; private set; } = 0;
        public int StepsCount { get; } = 16;
        public bool IsLooping { get; set; } = true;

        private AudioPlayer player = new AudioPlayer();

        public DrumMachine()
        {
            string[] defaultNames = { "Kick", "Snare", "HiHat", "Clap", "Tom", "Cymbal" };
            foreach (var name in defaultNames)
            {
                Tracks.Add(new DrumTrack(name, StepsCount));
            }
        }

        public void ToggleStep(int trackIndex, int stepIndex, bool value)
        {
            Tracks[trackIndex].Steps[stepIndex] = value;
        }

        public void PlayStep()
        {
            foreach (var track in Tracks)
            {
                if (track.Steps[CurrentStep])
                {
                    track.Play(player);
                }
            }

            CurrentStep = (CurrentStep + 1) % StepsCount;
        }
        public void AddTrack(string name)
        {
            Tracks.Add(new DrumTrack(name, StepsCount));
        }

        public void RemoveTrack(int index)
        {
            if (index >= 0 && index < Tracks.Count)
                Tracks.RemoveAt(index);
        }

        public void Reset()
        {
            CurrentStep = 0;
        }


        public float[] RenderToBuffer(int sampleRate = 44100)
        {
            int steps = StepsCount;
            double bpm = 120;

            double secondsPerStep = 60.0 / bpm / 4;
            int samplesPerStep = (int)(secondsPerStep * sampleRate);

            int totalSamples = samplesPerStep * steps;
            float[] buffer = new float[totalSamples];

            float peak = 0f;

            for (int step = 0; step < steps; step++)
            {
                int offset = step * samplesPerStep;

                foreach (var track in Tracks)
                {
                    if (track is DrumTrack drumTrack)
                    {
                        if (!drumTrack.Steps[step] || string.IsNullOrEmpty(drumTrack.SamplePath))
                            continue;

                        var sampleData = WavReader.ReadMono(drumTrack.SamplePath);

                        int maxLen = Math.Min(sampleData.Length, buffer.Length - offset);

                        for (int i = 0; i < maxLen; i++)
                        {
                            int index = offset + i;
                            if (index < buffer.Length)
                            {
                                float sample = sampleData[i] * (float)drumTrack.Volume * 0.3f;

                                buffer[index] += sample;

                                if (buffer[index] > 1f) buffer[index] = 1f;
                                if (buffer[index] < -1f) buffer[index] = -1f;

                                float abs = Math.Abs(buffer[index]);
                                if (abs > peak) peak = abs;
                            }
                        }
                    }
                }
            }

            if (peak > 1f)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] /= peak;
                }
            }

            return buffer;
        }
    }
}