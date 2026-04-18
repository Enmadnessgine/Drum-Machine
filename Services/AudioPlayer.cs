using System.Collections.Generic;
using System.Windows.Media;

namespace Drum_Machine.Services
{
    public class AudioPlayer
    {
        private List<MediaPlayer> activePlayers = new List<MediaPlayer>();

        public void Play(string path, float volume = 1.0f)
        {
            if (string.IsNullOrEmpty(path)) return;

            MediaPlayer player = new MediaPlayer();

            player.Open(new Uri(path, UriKind.Absolute));
            player.Volume = volume;
            player.Play();

            activePlayers.Add(player);

            player.MediaEnded += (s, e) =>
            {
                player.Close();
                activePlayers.Remove(player);
            };
        }
    }
}