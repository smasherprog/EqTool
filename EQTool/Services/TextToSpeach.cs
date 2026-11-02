using EQTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services
{
    public interface ITextToSpeach
    {
        void Say(string text);
    }
    public class TextToSpeach : ITextToSpeach
    {
#if !LINUX

        private const int synthCount = 8;
        private readonly List<Synth> synths = new List<Synth>();
        private volatile bool synthsBuilt;

        private class Synth
        {
            public System.Speech.Synthesis.SpeechSynthesizer Synthesizer;
            public System.Speech.Synthesis.Prompt LastPrompt;
        }
#endif
        private readonly EQToolSettings eQToolSettings;

        public TextToSpeach(EQToolSettings eQToolSettings)
        {
            this.eQToolSettings = eQToolSettings;
#if !LINUX
            // build them all up front so no alert waits for one to be created
            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    while (synths.Count < synthCount)
                    {
                        CreateSynth();
                    }
                }
                catch
                {
                }
                synthsBuilt = true;
            });
#endif
        }

#if !LINUX
        private void CreateSynth()
        {
            var s = new Synth { Synthesizer = new System.Speech.Synthesis.SpeechSynthesizer() };
            SelectVoice(s.Synthesizer, eQToolSettings.SelectedVoice);
            synths.Add(s);
            // Speaking once at zero volume loads the voice engine, so the first audible phrase is
            // not delayed while it warms up. Only the first needs it: once the engine is loaded,
            // a new synthesizer speaks as quickly as a warmed one.
            if (synths.Count == 1)
            {
                s.Synthesizer.Volume = 0;
                s.Synthesizer.Speak("t");
            }
        }

        private static void SelectVoice(System.Speech.Synthesis.SpeechSynthesizer synthesizer, string voice)
        {
            try
            {
                synthesizer.SelectVoice(voice);
            }
            catch (ArgumentException)
            {
                // a blank, missing or disabled voice throws here; a new synthesizer then keeps
                // the default voice
            }
        }
#endif

        public void Say(string text)
        {
#if !LINUX
            if (!synthsBuilt || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var s = synths.FirstOrDefault(a => a.LastPrompt == null || a.LastPrompt.IsCompleted);
            if (s == null)
            {
                return;
            }

            try
            {
                SelectVoice(s.Synthesizer, eQToolSettings.SelectedVoice);
                s.Synthesizer.Volume = eQToolSettings.GlobalAudioVolume ?? 100;
                // A power-saving audio device takes a moment to wake and drops the first
                // samples it is handed, clipping the start of the phrase.d
                var prompt = new System.Speech.Synthesis.PromptBuilder();
                prompt.AppendBreak(TimeSpan.FromMilliseconds(250));
                prompt.AppendText(text);
                s.LastPrompt = s.Synthesizer.SpeakAsync(prompt);
            }
            catch
            {
            }
#else
            var volume = eQToolSettings.GlobalAudioVolume ?? 100;
            if (string.IsNullOrWhiteSpace(text) || volume == 0)
            {
                return;
            }

            _ = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Process.Start(@"Z:\usr\bin\spd-say", new[] { "-P", "important", "-i", (volume * 2 - 100).ToString(System.Globalization.CultureInfo.InvariantCulture), "--", text })?.Dispose();
                }
                catch
                {
                }
            });
#endif
        }
    }
}
