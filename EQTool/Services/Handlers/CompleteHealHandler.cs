﻿using EQTool.Models;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQTool.Services.Handlers
{
    public class CompleteHealHandler : BaseHandler
    {
        private class ChainAudioData : ChainData
        {
            public DateTime UpdatedTime { get; set; } = DateTime.UtcNow;
            public string TargetName { get; set; }
        }

        private readonly List<ChainAudioData> chainDatas = new List<ChainAudioData>();

        public CompleteHealHandler(LogEvents logEvents, ActivePlayer activePlayer, EQToolSettings eQToolSettings, ITextToSpeach textToSpeach) : base(logEvents, activePlayer, eQToolSettings, textToSpeach)
        {
            this.logEvents.CompleteHealEvent += LogParser_CHEvent;
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
                textToSpeach.Say($"CH Warning");
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

    }
}