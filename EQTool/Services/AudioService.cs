using EQTool.Models;
using System;

namespace EQTool.Services
{
    public interface IAudioService
    {
        void Play(string soundFilePath);
    }

    // Plays a user-selected sound file (wav/mp3/etc.) using a WPF MediaPlayer.
    // MediaPlayer must be created and driven on the UI (dispatcher) thread.
    public class AudioService : IAudioService
    {
        private readonly IAppDispatcher appDispatcher;
        private readonly EQToolSettings settings;
        private System.Windows.Media.MediaPlayer player;

        public AudioService(IAppDispatcher appDispatcher, EQToolSettings settings)
        {
            this.appDispatcher = appDispatcher;
            this.settings = settings;
        }

        public void Play(string soundFilePath)
        {
            if (string.IsNullOrWhiteSpace(soundFilePath) || !System.IO.File.Exists(soundFilePath))
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                try
                {
                    if (player == null)
                    {
                        player = new System.Windows.Media.MediaPlayer();
                    }
                    player.Stop();
                    // MediaPlayer volume is 0.0-1.0 and defaults to 0.5; map the 0-100 master volume onto it.
                    player.Volume = (settings.GlobalAudioVolume ?? 100) / 100.0;
                    player.Open(new Uri(soundFilePath, UriKind.Absolute));
                    player.Play();
                }
                catch
                {
                    // ignore playback errors (bad path / unsupported codec)
                }
            });
        }
    }
}
