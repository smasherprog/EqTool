using EQTool.Models;
using EQTool.Services;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    // Reusable editor for a TriggerOutput (text settings + audio settings).
    // Bound via its DataContext, so the same control serves the Basic, Timer Ending
    // and Timer Ended outputs.
    public partial class TriggerOutputEditor : UserControl
    {
        // Set by the host when an enabled Display Text field is empty, so the text box can show a
        // red border. Defaults to false, so outputs that don't validate display text stay normal.
        public static readonly System.Windows.DependencyProperty DisplayTextInvalidProperty =
            System.Windows.DependencyProperty.Register(
                nameof(DisplayTextInvalid), typeof(bool), typeof(TriggerOutputEditor),
                new System.Windows.PropertyMetadata(false));

        public bool DisplayTextInvalid
        {
            get => (bool)GetValue(DisplayTextInvalidProperty);
            set => SetValue(DisplayTextInvalidProperty, value);
        }

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
            // Tests play through the shared services so they honor the selected voice
            // and the master volume from general settings, just like real alerts.
            if (!(DataContext is TriggerOutput output) || !(System.Windows.Application.Current is App app))
            {
                return;
            }

            if (output.AudioType == TriggerAudioType.SoundFile && !string.IsNullOrWhiteSpace(output.SoundFile))
            {
                app.ResolveService<IAudioService>().Play(output.SoundFile);
            }
            else if (output.AudioType == TriggerAudioType.TextToSpeech && !string.IsNullOrWhiteSpace(output.TtsText))
            {
                app.ResolveService<ITextToSpeach>().Say(output.TtsText, true);
            }
        }
    }
}
