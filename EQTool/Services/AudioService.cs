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
        private readonly LogEvents logEvents;
        private readonly ActivePlayer activePlayer;
        private readonly EQToolSettings eQToolSettings;
        private readonly List<ChainAudioData> chainDatas = new List<ChainAudioData>();

        public AudioService(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings)
        {
            this.logEvents = logEvents;
            this.activePlayer = activePlayer;
            this.eQToolSettings = eQToolSettings;
            this.logEvents.InvisEvent += LogParser_InvisEvent;
            this.logEvents.EnrageEvent += LogParser_EnrageEvent;
            this.logEvents.LevitateEvent += LogParser_LevEvent;
            this.logEvents.FTEEvent += LogParser_FTEEvent;
            this.logEvents.CharmBreakEvent += LogParser_CharmBreakEvent;
            this.logEvents.FailedFeignEvent += LogParser_FailedFeignEvent;
            this.logEvents.GroupInviteEvent += LogParser_GroupInviteEvent;
            this.logEvents.SpellCastEvent += LogParser_StartCastingEvent;
            this.logEvents.CompleteHealEvent += LogParser_CHEvent;
            this.logEvents.SpellWornOffOtherEvent += LogParser_SpellWornOtherOffEvent1;
            this.logEvents.ResistSpellEvent += LogParser_ResistSpellEvent;
            this.logEvents.TextToSpeechEvent += LogParser_TextToSpeechEvent;
        }

        private void LogParser_TextToSpeechEvent(object sender, TextToSpeechEvent textToSpeechEvent)
        {
            //var overlay = activePlayer?.Player?.ResistWarningAudio ?? false;
            //if (!overlay)
            //{
            //    return;
            //}
            //var target = e.isYou ? "You " : "Your target ";
            PlayResource(textToSpeechEvent.Text_phonetic);
        }

        private void LogParser_ResistSpellEvent(object sender, ResistSpellEvent e)
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

        private void LogParser_SpellWornOtherOffEvent1(object sender, SpellWornOffOtherEvent e)
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

        private void LogParser_CHEvent(object sender, CompleteHealEvent e)
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

        private ChainAudioData GetOrCreateChain(CompleteHealEvent e)
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

        private void LogParser_StartCastingEvent(object sender, SpellCastEvent e)
        {
            var overlay = activePlayer?.Player?.DragonRoarAudio ?? false;
            if (!overlay || e.Spell.name != "Dragon Roar")
            {
                return;
            }

            _ = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                PlayResource($"Dragon Roar out");
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

        private void LogParser_GroupInviteEvent(object sender, GroupInviteEvent e)
        {
            if (activePlayer?.Player?.GroupInviteAudio == true)
            {
                PlayResource($"{e.Inviter} Invites you to a group");
            }
        }

        private void LogParser_FailedFeignEvent(object sender, FailedFeignEvent e)
        {
            if (activePlayer?.Player?.FailedFeignAudio == true)
            {
                PlayResource($"{e.PersonWhoFailedFeign} Failed Feign Death");
            }
        }

        private void LogParser_CharmBreakEvent(object sender, CharmBreakEvent e)
        {
            if (activePlayer?.Player?.CharmBreakAudio == true)
            {
                PlayResource($"Charm Break");
            }
        }

        private void LogParser_FTEEvent(object sender, FTEEvent e)
        {
            if (activePlayer?.Player?.FTEAudio == true)
            {
                PlayResource($"{e.FTEPerson} F T E {e.NPCName}");
            }
        }

        private void LogParser_LevEvent(object sender, LevitateEvent e)
        {
            if (activePlayer?.Player?.LevFadingAudio == true && e.LevitateStatus == LevParser.LevStatus.Fading)
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

        private void LogParser_EnrageEvent(object sender, EnrageEvent e)
        {
            if (activePlayer?.Player?.EnrageAudio == true)
            {
                PlayResource($"{e.NpcName} is enraged.");
            }
        }

        private void LogParser_InvisEvent(object sender, InvisEvent e)
        {
            if (activePlayer?.Player?.InvisFadingAudio == true && e.InvisStatus == InvisParser.InvisStatus.Fading)
            {
                PlayResource($"Invisability Fading.");
            }
        }
    }
}
