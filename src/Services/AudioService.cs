﻿using EQTool.ViewModels;
using System.Speech.Synthesis;

namespace EQTool.Services
{
    public class AudioService
    {
        private readonly LogParser logParser;
        private readonly ActivePlayer activePlayer;

        public AudioService(LogParser logParser, ActivePlayer activePlayer)
        {
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            this.logParser.InvisEvent += LogParser_InvisEvent;
            this.logParser.EnrageEvent += LogParser_EnrageEvent;
            this.logParser.LevEvent += LogParser_LevEvent;
        }

        private void LogParser_LevEvent(object sender, LevParser.LevStatus e)
        {
            if (this.activePlayer?.Player?.LevFadingAudio == true)
            {
                this.PlayResource("Levitate Fading");
            }
        }

        private void PlayResource(string text)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                var synth = new SpeechSynthesizer();
                synth.SetOutputToDefaultAudioDevice();
                synth.Speak(text);
            });
        }

        private void LogParser_EnrageEvent(object sender, EnrageParser.EnrageEvent e)
        {
            if (this.activePlayer?.Player?.EnrageAudio == true)
            {
                this.PlayResource($"{e.NpcName} is enraged.");
            }
        }

        private void LogParser_InvisEvent(object sender, InvisParser.InvisStatus e)
        {
            if (this.activePlayer?.Player?.InvisFadingAudio == true)
            {
                this.PlayResource($"Invisability Fading.");
            }
        }
    }
}