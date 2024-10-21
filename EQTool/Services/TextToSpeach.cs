using EQTool.Models;

namespace EQTool.Services
{
    public interface ITextToSpeach
    {
        void Say(string text);
    }
    public class TextToSpeach: ITextToSpeach
    {
        private readonly EQToolSettings eQToolSettings;

        public TextToSpeach(EQToolSettings eQToolSettings)
        {
            this.eQToolSettings = eQToolSettings;
        }

        public void Say(string text)
        {
#if !LINUX
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var synth = new System.Speech.Synthesis.SpeechSynthesizer();
                if (string.IsNullOrWhiteSpace(eQToolSettings.SelectedVoice))
                {
                    synth.SetOutputToDefaultAudioDevice();
                }
                else
                {
                    synth.SelectVoice(eQToolSettings.SelectedVoice);
                }

                synth.Speak(text);
            });
#endif
        }
    }
}
