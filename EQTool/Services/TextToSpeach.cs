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
#if !LINUX
        // One synthesizer queues phrases instead of overlapping them, so each phrase goes to
        // an idle one and a busy pool grows a new one. The cap stops a burst of triggers from
        // spawning an unbounded number; past it phrases queue as they would on a single one.
        private const int maxSynths = 8;

        private readonly List<Synth> synths = new List<Synth>();

        private class Synth
        {
            public System.Speech.Synthesis.SpeechSynthesizer Synthesizer;
            // The selected voice is per synthesizer, so each tracks what it was last set to.
            public string LastSelectedVoice = string.Empty;
            // SpeakAsync returns before the synthesizer leaves the Ready state, so its own state
            // cannot say whether it has already been handed a phrase.
            public bool InUse;
        }
#endif
        private readonly EQToolSettings eQToolSettings;

        public TextToSpeach(EQToolSettings eQToolSettings)
        {
            this.eQToolSettings = eQToolSettings;
#if !LINUX
            // build the first one up front so the first alert is not delayed by it
            RunInBackground(() =>
            {
                lock (synths)
                {
                    _ = CreateSynth();
                }
            });
#endif
        }

#if !LINUX
        // Creating a synthesizer blocks while the voice engine loads and callers are on the UI
        // thread, so none of this runs inline. Failures are swallowed so one bad phrase (e.g. a
        // voice that failed to load) cannot kill speech for later alerts.
        private static void RunInBackground(Action work)
        {
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    work();
                }
                catch
                {
                }
            });
        }

        private Synth GetIdleSynth()
        {
            foreach (var s in synths)
            {
                if (!s.InUse)
                {
                    return s;
                }
            }
            // at the cap the oldest is reused, which queues the phrase behind whatever it is saying
            return synths.Count < maxSynths ? CreateSynth() : synths[0];
        }

        private Synth CreateSynth()
        {
            var s = new Synth { Synthesizer = new System.Speech.Synthesis.SpeechSynthesizer() };
            if (string.IsNullOrWhiteSpace(eQToolSettings.SelectedVoice))
            {
                s.Synthesizer.SetOutputToDefaultAudioDevice();
            }
            else
            {
                s.Synthesizer.SelectVoice(eQToolSettings.SelectedVoice);
                s.LastSelectedVoice = eQToolSettings.SelectedVoice;
            }
            // speaking once at zero volume loads the voice engine, so the first audible phrase
            // is not delayed while it warms up
            s.Synthesizer.Volume = 0;
            s.Synthesizer.Speak("test");
            s.Synthesizer.SpeakCompleted += (o, e) => s.InUse = false;
            synths.Add(s);
            return s;
        }

        // A synthesizer is not thread-safe, and unsynchronized access let phrases speak at another
        // phrase's volume, so picking one and setting it up is a single critical section. Nothing
        // in here waits for speech: the audio renders on the synthesizer's own queue.
        private void Speak(string text, string voice, int volume)
        {
            lock (synths)
            {
                var s = GetIdleSynth();
                if (string.IsNullOrWhiteSpace(voice) && s.LastSelectedVoice != string.Empty)
                {
                    s.Synthesizer.SetOutputToDefaultAudioDevice();
                    s.LastSelectedVoice = string.Empty;
                }
                else if (!string.IsNullOrWhiteSpace(voice) && s.LastSelectedVoice != voice)
                {
                    s.Synthesizer.SelectVoice(voice);
                    s.LastSelectedVoice = voice;
                }

                s.Synthesizer.Volume = volume;
                // A power-saving audio device takes a moment to wake and drops the first
                // samples it is handed, clipping the start of the phrase.
                var prompt = new System.Speech.Synthesis.PromptBuilder();
                prompt.AppendBreak(TimeSpan.FromMilliseconds(250));
                prompt.AppendText(text);
                s.Synthesizer.SpeakAsync(prompt);
                // set last, so a synthesizer that failed to take the phrase is not lost from the pool
                s.InUse = true;
            }
        }
#endif

        public void Say(string text)
        {
#if !LINUX
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // read once here so a settings change partway through cannot split a phrase's voice and volume
            var voice = eQToolSettings.SelectedVoice;
            var volume = eQToolSettings.GlobalAudioVolume ?? 100;
            RunInBackground(() => Speak(text, voice, volume));
#endif
        }
    }
}
