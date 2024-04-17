using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using static EQTool.Services.EventsList;

namespace EQTool.Services
{
    public class ChainAudioData : ChainData
    {
        public DateTime UpdatedTime { get; set; } = DateTime.UtcNow;
        public string TargetName { get; set; }
    }

    public class AudioService
    {
        private readonly EventsList eventsList;
        private readonly ActivePlayer activePlayer;
        private readonly EQToolSettings eQToolSettings;
        private readonly List<ChainAudioData> chainDatas = new List<ChainAudioData>();

        public AudioService(EventsList eventsList, ActivePlayer activePlayer, EQToolSettings eQToolSettings)
        {
            this.eventsList = eventsList;
            this.activePlayer = activePlayer;
            this.eQToolSettings = eQToolSettings;
            this.eventsList.InvisEvent += LogParser_InvisEvent;
            this.eventsList.EnrageEvent += LogParser_EnrageEvent;
            this.eventsList.LevEvent += LogParser_LevEvent;
            this.eventsList.FTEEvent += LogParser_FTEEvent;
            this.eventsList.CharmBreakEvent += LogParser_CharmBreakEvent;
            this.eventsList.FailedFeignEvent += LogParser_FailedFeignEvent;
            this.eventsList.GroupInviteEvent += LogParser_GroupInviteEvent;
            this.eventsList.StartCastingEvent += LogParser_StartCastingEvent;
            this.eventsList.CHEvent += LogParser_CHEvent;
            this.eventsList.SpellWornOtherOffEvent += LogParser_SpellWornOtherOffEvent1;
            this.eventsList.ResistSpellEvent += LogParser_ResistSpellEvent;
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellParser.ResistSpellData e)
        {
            var overlay = this.activePlayer?.Player?.ResistWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }
            var target = e.isYou ? "You " : "Your target ";
            this.PlayResource($"{target} resisted the {e.Spell.name} spell");
        }

        private List<string> RootSpells = new List<string>()
        {
            "Root",
            "Fetter",
            "Enstill",
            "Immobalize",
            "Paralyzing Earth",
            "Grasping Roots",
            "Ensnaring Roots",
            "Enveloping Roots",
            "Engulfing Roots",
            "Engorging Roots",
            "Entrapping Roots"
        };

        private void LogParser_SpellWornOtherOffEvent1(object sender, SpellWornOffOtherEventArgs e)
        {
            var overlay = this.activePlayer?.Player?.RootWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }

            if (RootSpells.Any(a => string.Equals(a, e.SpellName, StringComparison.OrdinalIgnoreCase)))
            {
                this.PlayResource($"{e.SpellName} has worn off!");
            }
        }

        private void LogParser_CHEvent(object sender, Parsing.ChParser.ChParseData e)
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

        private ChainAudioData GetOrCreateChain(Parsing.ChParser.ChParseData e)
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

        private void LogParser_StartCastingEvent(object sender, SpellEventArgs e)
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

        private void LogParser_CharmBreakEvent(object sender, CharmBreakArgs e)
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
                if (string.IsNullOrWhiteSpace(this.eQToolSettings.SelectedVoice))
                {
                    synth.SetOutputToDefaultAudioDevice();
                }
                else
                {
                    synth.SelectVoice(this.eQToolSettings.SelectedVoice);
                }

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
