using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;

namespace EQTool.Services
{
    public class ChainAudioData : ChainData
    {
        public DateTime UpdatedTime { get; set; } = DateTime.UtcNow;
        public string TargetName { get; set; }
    }

    public class AudioService
    {
        private readonly LogParser logParser;
        private readonly ActivePlayer activePlayer;
        private readonly List<ChainAudioData> chainDatas = new List<ChainAudioData>();

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
            this.logParser.CHEvent += LogParser_CHEvent;
        }

        private void LogParser_CHEvent(object sender, ChParser.ChParseData e)
        {
            var overlay = this.activePlayer?.Player?.ChChainWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }

            var chaindata = this.GetOrCreateChain(e);
            var shouldwarn = CHService.ShouldWarnOfChain(chaindata, e);
            if (shouldwarn)
            {
                this.PlayResource($"CH Warning");
            }
        }

        private ChainAudioData GetOrCreateChain(ChParser.ChParseData e)
        {
            var d = DateTime.UtcNow;
            var toremove = this.chainDatas.Where(a => (d - a.UpdatedTime).TotalSeconds > 20).ToList();
            foreach (var item in toremove)
            {
                this.chainDatas.Remove(item);
            }

            var f = this.chainDatas.FirstOrDefault(a => a.TargetName == e.Recipient);
            if (f == null)
            {
                f = new ChainAudioData
                {
                    UpdatedTime = d,
                    TargetName = e.Recipient
                };
                this.chainDatas.Add(f);
            }
            f.UpdatedTime = d;
            return f;
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
