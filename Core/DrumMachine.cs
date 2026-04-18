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
    }
}