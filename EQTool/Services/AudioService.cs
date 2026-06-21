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
        private System.Windows.Media.MediaPlayer player;

        public AudioService(IAppDispatcher appDispatcher)
        {
            this.appDispatcher = appDispatcher;
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
