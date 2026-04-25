using System.IO;
using System.Collections.Generic;
using Drum_Machine.Models;
using Drum_Machine.Services;

namespace Drum_Machine.Core
{
    public class DrumMachine
    {
        public List<Track> Tracks { get; set; } = new List<Track>();
        public int CurrentStep { get; private set; } = 0;
        public int StepsCount { get; } = 16;
        public bool IsLooping { get; set; } = true;

        private AudioPlayer player = new AudioPlayer();

        public DrumMachine()
        {
            Tracks = new List<Track>
            {
                new Track("Kick", StepsCount),
                new Track("Snare", StepsCount),
                new Track("HiHat", StepsCount),
                new Track("Clap", StepsCount),
                new Track("Tom", StepsCount),
                new Track("Cymbal", StepsCount)
            };
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
                    player.Play(track.SamplePath, (float)track.Volume);
                }
            }

            CurrentStep++;

            if (CurrentStep >= StepsCount)
            {
                if (IsLooping)
                    CurrentStep = 0;
                else
                    CurrentStep = StepsCount - 1;
            }
        }
        public void AddTrack(string name)
        {
            Tracks.Add(new Track(name, StepsCount));
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
                    if (!track.Steps[step] || string.IsNullOrEmpty(track.SamplePath))
                        continue;

                    var sampleData = WavReader.ReadMono(track.SamplePath);

                    int maxLen = Math.Min(sampleData.Length, buffer.Length - offset);

                    for (int i = 0; i < maxLen; i++)
                    {
                        int index = offset + i;

                        if (index < buffer.Length)
                        {
                            float sample = sampleData[i] * (float)track.Volume * 0.3f;

                            buffer[index] += sample;

                            if (buffer[index] > 1f) buffer[index] = 1f;
                            if (buffer[index] < -1f) buffer[index] = -1f;

                            float abs = Math.Abs(buffer[index]);
                            if (abs > peak) peak = abs;
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