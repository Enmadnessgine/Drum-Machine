namespace Drum_Machine.Models
{
    public class DrumTrack : BaseTrack
    {
        public string SamplePath { get; set; } = "";

        public DrumTrack(string name, int stepCount) : base(name, stepCount)
        {

        }

        public override void Play(Services.AudioPlayer player)
        {
            if (!string.IsNullOrEmpty(SamplePath))
            {
                player.Play(SamplePath, (float)Volume);
            }
        }
    }
}