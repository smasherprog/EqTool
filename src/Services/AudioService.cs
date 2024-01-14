using EQTool.ViewModels;
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
            this.logParser.FTEEvent += LogParser_FTEEvent;
            this.logParser.CharmBreakEvent += LogParser_CharmBreakEvent;
            this.logParser.FailedFeignEvent += LogParser_FailedFeignEvent;
            this.logParser.GroupInviteEvent += LogParser_GroupInviteEvent;
            this.logParser.StartCastingEvent += LogParser_StartCastingEvent;
        }

        private void LogParser_StartCastingEvent(object sender, LogParser.SpellEventArgs e)
        {
            var overlay = this.activePlayer?.Player?.DragonRoarAudio ?? false;
            if (!overlay || e.Spell.Spell.name != "Dragon Roar")
            {
                return;
            }

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000 * 30);
                this.PlayResource($"Dragon Roar in 6 Seconds!");
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(1000);
                this.PlayResource($"4 Seconds!");
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(1000);
                this.PlayResource($"2");
                System.Threading.Thread.Sleep(1000);
                this.PlayResource($"1");
                System.Threading.Thread.Sleep(1000);
            });
        }

        private void LogParser_GroupInviteEvent(object sender, string e)
        {
            if (this.activePlayer?.Player?.GroupInviteAudio == true)
            {
                this.PlayResource(e);
            }
        }

        private void LogParser_FailedFeignEvent(object sender, string e)
        {
            if (this.activePlayer?.Player?.FailedFeignAudio == true)
            {
                this.PlayResource($"Failed Feign Death");
            }
        }

        private void LogParser_CharmBreakEvent(object sender, LogParser.CharmBreakArgs e)
        {
            if (this.activePlayer?.Player?.CharmBreakAudio == true)
            {
                this.PlayResource($"Charm Break");
            }
        }

        private void LogParser_FTEEvent(object sender, FTEParser.FTEParserData e)
        {
            if (this.activePlayer?.Player?.FTEAudio == true)
            {
                this.PlayResource($"{e.FTEPerson} F T E {e.NPCName}");
            }
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
