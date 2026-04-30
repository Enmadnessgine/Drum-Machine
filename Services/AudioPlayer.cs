using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace Drum_Machine.Services
{
    public class AudioPlayer
    {
        private List<MediaPlayer> activePlayers = new List<MediaPlayer>();

        public void Play(string path, float volume = 1.0f)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string absolutePath = path;
                if (!Path.IsPathRooted(path))
                {
                    absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                }

                MediaPlayer player = new MediaPlayer();

                player.Open(new Uri(absolutePath, UriKind.Absolute));
                player.Volume = volume;
                player.Play();

                activePlayers.Add(player);

                player.MediaEnded += (s, e) =>
                {
                    player.Close();
                    activePlayers.Remove(player);
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка відтворення аудіо: {ex.Message}");
            }
        }
    }
}