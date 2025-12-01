using Autofac;
using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQtoolsTests
{
    public class BaseTestClass
    {
        protected readonly IContainer container;
        protected readonly ActivePlayer player;
        public BaseTestClass()
        {
            container = DI.Init();
            player = container.Resolve<ActivePlayer>();
            player.Player = new PlayerInfo
            {
                Level = 20,
                PlayerClass = PlayerClasses.Cleric,
                Zone = "templeveeshan",
                Name = "pigy"
            };
        }
    }

    public static class LogParserExtension
    {
        public static DateTime PushAuthenticLocalWho(this LogParser logParser, string zone, PlayerInfo playerInZone, DateTime? dateTime = null)
            => logParser.PushAuthenticLocalWho(zone, new List<PlayerInfo> {playerInZone}, dateTime);
        
        public static DateTime PushAuthenticLocalWho(this LogParser logParser, string zone, IEnumerable<PlayerInfo> playersInZone, DateTime? dateTime = null)
        {
            var timeOfLog = dateTime ?? logParser.LastEntryDateTime;
            logParser.Push("Players on EverQuest:", timeOfLog);
            logParser.Push("---------------------------", timeOfLog);

            foreach (var playerInfo in playersInZone)
            {
                if (string.IsNullOrWhiteSpace(playerInfo.GuildName))
                    logParser.Push($"[{playerInfo.Level} {playerInfo.PlayerClass}] {playerInfo.Name} (Human)", timeOfLog); //TODO: Not always human
                else
                    logParser.Push($"[{playerInfo.Level} {playerInfo.PlayerClass}] {playerInfo.Name} (Human) <{playerInfo.GuildName}>", timeOfLog); //TODO: Not always human
            }
            
            var playerCount = playersInZone.Count();
            if (playerCount == 1)
                logParser.Push($"There is 1 player in {zone}.", timeOfLog);
            else
                logParser.Push($"There are {playerCount} players in {zone}.", timeOfLog);
            
            return timeOfLog.Add(TimeSpan.FromSeconds(1));
        }
        
        public static DateTime PushAuthenticGlobalWho(this LogParser logParser, string zone, PlayerInfo playerInZone, DateTime? dateTime = null)
            => logParser.PushAuthenticGlobalWho(zone, new List<PlayerInfo> {playerInZone}, dateTime);
        
        public static DateTime PushAuthenticGlobalWho(this LogParser logParser, string zone, IEnumerable<PlayerInfo> playersInZone, DateTime? dateTime = null)
        {
            var timeOfLog = dateTime ?? logParser.LastEntryDateTime;
            logParser.Push("Players in EverQuest:", timeOfLog);
            logParser.Push("---------------------------", timeOfLog);
            
            foreach (var playerInfo in playersInZone)
            {
                if (string.IsNullOrWhiteSpace(playerInfo.GuildName))
                    logParser.Push($"[{playerInfo.Level} {playerInfo.PlayerClass}] {playerInfo.Name} (Human) ZONE: {zone}", timeOfLog); //TODO: Not always human
                else
                    logParser.Push($"[{playerInfo.Level} {playerInfo.PlayerClass}] {playerInfo.Name} (Human) <{playerInfo.GuildName}> ZONE: {zone}", timeOfLog); //TODO: Not always human
            }
            
            var playerCount = playersInZone.Count();
            if (playerCount == 1)
                logParser.Push($"There is 1 player in Everquest.", timeOfLog);
            else
                logParser.Push($"There are {playerCount} players in Everquest.", timeOfLog);
            
            return timeOfLog.Add(TimeSpan.FromSeconds(1));
        }
        
        public static DateTime PushAuthenticSpellCast(this LogParser logParser, Spell spell, string target, string caster = null, DateTime? dateTime = null)
            => logParser.PushAuthenticAoESpellCast(spell, new List<string> { target }, caster, dateTime: dateTime);
        
        public static DateTime PushAuthenticAoESpellCast(this LogParser logParser, Spell spell, List<string> targets, string caster = null, DateTime? dateTime = null)
        {
            var timeOfLog = dateTime ?? logParser.LastEntryDateTime;
            if (spell.casttime > 0 && !string.IsNullOrWhiteSpace(caster))
            {
                if (IsYou(caster))
                    logParser.Push($"You begin casting {spell.name}", timeOfLog);
                else
                    logParser.Push($"{caster} begins to cast a spell.", timeOfLog);
                
                timeOfLog += TimeSpan.FromSeconds(spell.casttime);
            }

            foreach (var target in targets)
                logParser.Push(IsYou(target) ? spell.cast_on_you : $"{target} {spell.cast_on_other}", timeOfLog);
            
            return timeOfLog;
        }

        public static void Push(this LogParser logParser, SpellWindowViewModel spellWindowViewModel, string message, DateTime datetime)
        {
            var logStyleDateTime = ToLogStyleDateTime(datetime);
            Debug.WriteLine($"BEG datetime {logStyleDateTime} logdatetime {logParser.LastEntryDateTime}");
            UpdateTriggers(logParser, spellWindowViewModel, datetime, message);
            logParser.Push(message, datetime);
            Debug.WriteLine($"END datetime {logStyleDateTime} logdatetime {logParser.LastEntryDateTime}");
        }

        public static void UpdateTriggers(this LogParser logParser, SpellWindowViewModel spellWindowViewModel, DateTime datetime, string reason = null)
        {
            var logStyleDateTime = ToLogStyleDateTime(datetime);
            var diff = logStyleDateTime - logParser.LastEntryDateTime;
            
            if (diff.TotalMilliseconds >= 1000)
            {
                Debug.WriteLine($"Fast forwarding {diff.TotalMilliseconds}ms" + (string.IsNullOrWhiteSpace(reason) ? "" : $"for {reason}"));
                var totaltime = diff.TotalMilliseconds;
                var timeslices = totaltime / 100;
                for (var time = 0; time < timeslices; time++)
                {
                    totaltime -= 100;
                    spellWindowViewModel.UpdateTriggers(100);
                }

                if (totaltime > 0)
                    spellWindowViewModel.UpdateTriggers(totaltime);
            }
        }

        //this parsing is needed because the logger will truncate anything beyond a seconds worth of time
        private static DateTime ToLogStyleDateTime(DateTime datetime) => DateTime.Parse(datetime.ToString("G"));
        
        private static bool IsYou(string caster) => caster == EQSpells.SpaceYou || caster == EQSpells.You;
    }
}
