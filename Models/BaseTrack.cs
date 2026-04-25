using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drum_Machine.Models
{
    public abstract class BaseTrack
    {
        public string Name { get; set; } = "Track";
        public bool[] Steps { get; set; }
        public double Volume { get; set; } = 1.0;

        protected BaseTrack(string name, int stepCount)
        {
            Name = name;
            Steps = new bool[stepCount];
        }

        public abstract void Play(Services.AudioPlayer player);
    }
}