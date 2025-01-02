using EQTool.Models;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace EQTool.Services
{
    public interface ITextToSpeach
    {
        void Say(string text);
    }
    public class TextToSpeach: ITextToSpeach
    {
        // keep a dictionary of recent audio alert phrases, key = phrase to be spoken, value = DateTime of last occurence.
        // If the same phrase comes again inside audioAlertCooldown seconds, stay silent 
        private int audioAlertCooldownSeconds = 5;
        private Dictionary<string, DateTime> audioAlertHistory = new Dictionary<string, DateTime>();

        private readonly EQToolSettings eQToolSettings;

        public TextToSpeach(EQToolSettings eQToolSettings)
        {
            this.eQToolSettings = eQToolSettings;
        }

        public void Say(string text)
        {
#if !LINUX
            // is this phrase not in the history?
            DateTime now = DateTime.Now;
            bool shouldSpeak = false;
            if (audioAlertHistory.ContainsKey(text) == false)
            {
                shouldSpeak = true;
                audioAlertHistory.Add(text, now);
            }
            else
            {
                // the history has an entry for this phrase.  Let's see how old it is
                DateTime prior = audioAlertHistory[text];
                TimeSpan elapsed = now - prior;
                if (elapsed.TotalSeconds > audioAlertCooldownSeconds)
                {
                    // update the time stamp for this phrase
                    shouldSpeak = true;
                    audioAlertHistory[text] = now;
                }
            }

            if (shouldSpeak)
            {
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
            }
#endif
        }
    }
}
