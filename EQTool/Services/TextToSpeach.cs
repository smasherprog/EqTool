using EQTool.Models;
using System;
using System.Collections.Generic;

namespace EQTool.Services
{
    public interface ITextToSpeach
    {
        void Say(string text);
        // When interrupt is true, any in-progress speech is cancelled and the
        // repeat-suppression cooldown is bypassed so the new phrase speaks immediately.
        void Say(string text, bool interrupt);
    }
    public class TextToSpeach : ITextToSpeach
    {
        // keep a dictionary of recent audio alert phrases, key = phrase to be spoken, value = DateTime of last occurence.
        // If the same phrase comes again inside audioAlertCooldown seconds, stay silent 
        private const int audioAlertCooldownSeconds = 5;
        private readonly Dictionary<string, DateTime> audioAlertHistory = new Dictionary<string, DateTime>();

#if !LINUX
        // All synthesizer access runs on this serial task chain, in Say() call order.
        // The synthesizer is not thread-safe, and unsynchronized access let phrases speak
        // at the warm-up volume or let racing interrupts cancel each other. The chain only
        // serializes the short configure-and-submit sections; the audio itself renders on
        // the synthesizer's own internal queue, so nothing here waits for speech to finish.
        private readonly object chainLock = new object();
        private System.Threading.Tasks.Task synthWorkChain = System.Threading.Tasks.Task.CompletedTask;
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
            EnqueueSynthWork(() =>
            {
                synth.Volume = 0;
                synth.Speak("test");
            });
#endif
        }

#if !LINUX
        // Appends work to the serial chain. Failures are swallowed per item so one bad
        // phrase (e.g. a voice that failed to load) cannot kill speech for all later alerts.
        private void EnqueueSynthWork(Action work)
        {
            lock (chainLock)
            {
                synthWorkChain = synthWorkChain.ContinueWith(_ =>
                {
                    try
                    {
                        work();
                    }
                    catch
                    {
                    }
                }, System.Threading.Tasks.TaskScheduler.Default);
            }
        }
#endif

        public void Say(string text)
        {
            Say(text, false);
        }

        public void Say(string text, bool interrupt)
        {
#if !LINUX
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // is this phrase not in the history?
            var now = DateTime.Now;
            var shouldSpeak = false;
            lock (audioAlertHistory)
            {
                if (interrupt)
                {
                    // interrupting speech bypasses the repeat-suppression cooldown
                    shouldSpeak = true;
                    audioAlertHistory[text] = now;
                }
                else if (audioAlertHistory.ContainsKey(text) == false)
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
            }

            if (shouldSpeak)
            {
                EnqueueSynthWork(() =>
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

                    synth.Volume = eQToolSettings.GlobalAudioVolume ?? 100;
                    if (interrupt)
                    {
                        synth.SpeakAsyncCancelAll();
                    }
                    // A power-saving audio device takes a moment to wake and drops the
                    // first samples it is handed, clipping the start of the phrase.
                    // Leading silence gives it time to wake before the words begin.
                    var prompt = new System.Speech.Synthesis.PromptBuilder();
                    prompt.AppendBreak(TimeSpan.FromMilliseconds(250));
                    prompt.AppendText(text);
                    synth.SpeakAsync(prompt);
                });
            }
#endif
        }
    }
}
