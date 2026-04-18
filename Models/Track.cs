namespace Drum_Machine.Models
{
    public class Track
    {
        public string Name { get; set; } = "Track";
        public string SamplePath { get; set; } = "";
        public bool[] Steps { get; set; } = new bool[16];
        public double Volume { get; set; } = 1.0;
        public Track(string name, int stepCount)
        {
            Name = name;
            Steps = new bool[stepCount];
        }
    }
}