using EQTool.Models;
using EQTool.Services.Parsing;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQToolSettings eQToolSettings;
        private readonly List<ChainAudioData> chainDatas = new List<ChainAudioData>();

        public AudioService(LogEvents logEvents, LogParser logParser, ActivePlayer activePlayer, EQToolSettings eQToolSettings)
        {
            this.logEvents = logEvents;
            this.logParser = logParser;
            this.activePlayer = activePlayer;
            this.eQToolSettings = eQToolSettings;
            this.logParser.InvisEvent += LogParser_InvisEvent;
            this.logParser.EnrageEvent += LogParser_EnrageEvent;
            this.logParser.LevEvent += LogParser_LevEvent;
            this.logEvents.FTEEvent += LogParser_FTEEvent;
            this.logParser.CharmBreakEvent += LogParser_CharmBreakEvent;
            this.logParser.FailedFeignEvent += LogParser_FailedFeignEvent;
            this.logParser.GroupInviteEvent += LogParser_GroupInviteEvent;
            this.logParser.StartCastingEvent += LogParser_StartCastingEvent;
            this.logParser.CHEvent += LogParser_CHEvent;
            this.logParser.SpellWornOtherOffEvent += LogParser_SpellWornOtherOffEvent1;
            this.logParser.ResistSpellEvent += LogParser_ResistSpellEvent;
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellParser.ResistSpellData e)
        {
            var overlay = activePlayer?.Player?.ResistWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }
            var target = e.isYou ? "You " : "Your target ";
            PlayResource($"{target} resisted the {e.Spell.name} spell");
        }

        private readonly List<string> RootSpells = new List<string>()
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

        private void LogParser_SpellWornOtherOffEvent1(object sender, LogParser.SpellWornOffOtherEventArgs e)
        {
            var overlay = activePlayer?.Player?.RootWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }

            if (RootSpells.Any(a => string.Equals(a, e.SpellName, StringComparison.OrdinalIgnoreCase)))
            {
                PlayResource($"{e.SpellName} has worn off!");
            }
        }

        private void LogParser_CHEvent(object sender, ChParser.ChParseData e)
        {
            var overlay = activePlayer?.Player?.ChChainWarningAudio ?? false;
            if (!overlay)
            {
                return;
            }

            var chaindata = GetOrCreateChain(e);
            var shouldwarn = CHService.ShouldWarnOfChain(chaindata, e);
            if (shouldwarn)
            {
                PlayResource($"CH Warning");
            }
        }

        private ChainAudioData GetOrCreateChain(ChParser.ChParseData e)
        {
            var d = DateTime.UtcNow;
            var toremove = chainDatas.Where(a => (d - a.UpdatedTime).TotalSeconds > 20).ToList();
            foreach (var item in toremove)
            {
                _ = chainDatas.Remove(item);
            }

            var f = chainDatas.FirstOrDefault(a => a.TargetName == e.Recipient);
            if (f == null)
            {
                f = new ChainAudioData
                {
                    UpdatedTime = d,
                    TargetName = e.Recipient
                };
                chainDatas.Add(f);
            }
            f.UpdatedTime = d;
            return f;
        }

        private void LogParser_StartCastingEvent(object sender, LogParser.SpellEventArgs e)
        {
            var overlay = activePlayer?.Player?.DragonRoarAudio ?? false;
            if (!overlay || e.Spell.Spell.name != "Dragon Roar")
            {
                return;
            }

            _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000 * 30);
                PlayResource($"Dragon Roar in 6 Seconds!");
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(1000);
                PlayResource($"4 Seconds!");
                System.Threading.Thread.Sleep(1000);
                System.Threading.Thread.Sleep(1000);
                PlayResource($"2");
                System.Threading.Thread.Sleep(1000);
                PlayResource($"1");
                System.Threading.Thread.Sleep(1000);
            });
        }

        private void LogParser_GroupInviteEvent(object sender, string e)
        {
            if (activePlayer?.Player?.GroupInviteAudio == true)
            {
                PlayResource(e);
            }
        }

        private void LogParser_FailedFeignEvent(object sender, string e)
        {
            if (activePlayer?.Player?.FailedFeignAudio == true)
            {
                PlayResource($"Failed Feign Death");
            }
        }

        private void LogParser_CharmBreakEvent(object sender, LogParser.CharmBreakArgs e)
        {
            if (activePlayer?.Player?.CharmBreakAudio == true)
            {
                PlayResource($"Charm Break");
            }
        }

        private void LogParser_FTEEvent(object sender, FTEParserData e)
        {
            if (activePlayer?.Player?.FTEAudio == true)
            {
                PlayResource($"{e.FTEPerson} F T E {e.NPCName}");
            }
        }

        private void LogParser_LevEvent(object sender, LevParser.LevStatus e)
        {
            if (activePlayer?.Player?.LevFadingAudio == true)
            {
                PlayResource("Levitate Fading");
            }
        }

        private void PlayResource(string text)
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

        private void LogParser_EnrageEvent(object sender, EnrageParser.EnrageEvent e)
        {
            if (activePlayer?.Player?.EnrageAudio == true)
            {
                PlayResource($"{e.NpcName} is enraged.");
            }
        }

        private void LogParser_InvisEvent(object sender, InvisParser.InvisStatus e)
        {
            if (activePlayer?.Player?.InvisFadingAudio == true)
            {
                PlayResource($"Invisability Fading.");
            }
        }
    }
}
