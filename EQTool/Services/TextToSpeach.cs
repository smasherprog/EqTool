using EQTool.Models;
using System;
using System.Collections.Generic;

namespace EQTool.Services
{
    public interface ITextToSpeach
    {
        void Say(string text);
    }
    public class TextToSpeach : ITextToSpeach
    {
        // keep a dictionary of recent audio alert phrases, key = phrase to be spoken, value = DateTime of last occurence.
        // If the same phrase comes again inside audioAlertCooldown seconds, stay silent 
        private const int audioAlertCooldownSeconds = 5;
        private readonly Dictionary<string, DateTime> audioAlertHistory = new Dictionary<string, DateTime>();

#if !LINUX
        private readonly System.Speech.Synthesis.SpeechSynthesizer synth;
        private string LastSelectedVoice = string.Empty;
#endif
        private readonly EQToolSettings eQToolSettings;

        public TextToSpeach(EQToolSettings eQToolSettings)
        {
            this.eQToolSettings = eQToolSettings;
#if !LINUX

            synth = new System.Speech.Synthesis.SpeechSynthesizer();
            if (string.IsNullOrWhiteSpace(eQToolSettings.SelectedVoice))
            {
                synth.SetOutputToDefaultAudioDevice();
            }
            else
            {
                synth.SelectVoice(eQToolSettings.SelectedVoice);
                LastSelectedVoice = eQToolSettings.SelectedVoice;
            }
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var previousVolume = synth.Volume;
                synth.Volume = 1;
                synth.Speak("test");
                synth.Volume = previousVolume;
            });
#endif
        }

        public void Say(string text)
        {
#if !LINUX
            // is this phrase not in the history?
            var now = DateTime.Now;
            var shouldSpeak = false;
            if (audioAlertHistory.ContainsKey(text) == false)
            {
                shouldSpeak = true;
                audioAlertHistory.Add(text, now);
            }
            else
            {
                // the history has an entry for this phrase.  Let's see how old it is
                var prior = audioAlertHistory[text];
                var elapsed = now - prior;
                if (elapsed.TotalSeconds > audioAlertCooldownSeconds)
                {
                    // update the time stamp for this phrase
                    shouldSpeak = true;
                    audioAlertHistory[text] = now;
                }
            }

            if (shouldSpeak)
            {
                if (string.IsNullOrWhiteSpace(eQToolSettings.SelectedVoice) && LastSelectedVoice != string.Empty)
                {
                    synth.SetOutputToDefaultAudioDevice();
                }
                else if (!string.IsNullOrWhiteSpace(eQToolSettings.SelectedVoice) && LastSelectedVoice != eQToolSettings.SelectedVoice)
                {
                    synth.SelectVoice(eQToolSettings.SelectedVoice);
                    LastSelectedVoice = eQToolSettings.SelectedVoice;
                }

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    synth.Speak(text);
                });
            }
#endif
        }
    }
}
