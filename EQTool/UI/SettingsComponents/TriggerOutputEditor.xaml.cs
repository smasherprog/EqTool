using EQTool.Models;
using System;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    // Reusable editor for a TriggerOutput (text settings + audio settings).
    // Bound via its DataContext, so the same control serves the Basic, Timer Ending
    // and Timer Ended outputs.
    public partial class TriggerOutputEditor : UserControl
    {
        private System.Windows.Media.MediaPlayer testPlayer;

        public TriggerOutputEditor()
        {
            InitializeComponent();
        }

        private void BrowseSoundFile(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(DataContext is TriggerOutput output))
            {
                return;
            }
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Audio Files|*.wav;*.mp3;*.wma;*.m4a;*.aac|All Files|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                output.SoundFile = dialog.FileName;
            }
        }

        private void TestAudio(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(DataContext is TriggerOutput output))
            {
                return;
            }

            if (output.AudioType == TriggerAudioType.SoundFile && !string.IsNullOrWhiteSpace(output.SoundFile) && System.IO.File.Exists(output.SoundFile))
            {
                try
                {
                    if (testPlayer == null)
                    {
                        testPlayer = new System.Windows.Media.MediaPlayer();
                    }
                    testPlayer.Stop();
                    testPlayer.Open(new Uri(output.SoundFile, UriKind.Absolute));
                    testPlayer.Play();
                }
                catch
                {
                }
            }
            else if (output.AudioType == TriggerAudioType.TextToSpeech && !string.IsNullOrWhiteSpace(output.TtsText))
            {
#if !LINUX
                try
                {
                    var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                    synth.SpeakAsync(output.TtsText);
                }
                catch
                {
                }
#endif
            }
        }
    }
}
