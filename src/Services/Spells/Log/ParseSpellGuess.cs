using EQTool.Models;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EQTool.Services.Spells.Log
{
    public class ParseSpellGuess
    {
        private readonly ActivePlayer activePlayer;
        private readonly EQSpells spells;
        private readonly List<string> IgnoreSpellsForGuesses = new List<string>(){
            "Tigir's Insects"
        };

        public ParseSpellGuess(ActivePlayer activePlayer, EQSpells spells)
        {
            this.activePlayer = activePlayer;
            this.spells = spells;
        }

        public SpellParsingMatch HandleBestGuessSpell(string message)
        {
            var removename = message.IndexOf("'");
            if (removename != -1)
            {
                var spellmessage = message.Substring(removename).Trim();
                if (spells.CastOtherSpells.TryGetValue(spellmessage, out var foundspells))
                {
                    foundspells = foundspells.Where(a => !IgnoreSpellsForGuesses.Contains(a.name)).ToList();
                    var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                    var targetname = message.Replace(foundspell.cast_on_other, string.Empty).Trim();
                    Debug.WriteLine($"Other Spell: {foundspell.name} Message: {spellmessage}");
                    var multiplematches = foundspell.Classes.All(a => a.Value == 255) && foundspells.Count > 1;
                    return new SpellParsingMatch
                    {
                        Spell = foundspell,
                        TargetName = targetname,
                        MultipleMatchesFound = multiplematches
                    };
                }
            }
            else
            {
                var match = Match(message, string.Empty);
                if (match != null)
                {
                    return match;
                }

                removename = message.IndexOf(" ");
                if (removename != -1)
                {
                    var firstpart = message.Substring(0, removename + 1);
                    var spellmessage = message.Substring(removename).Trim();
                    match = Match(spellmessage, firstpart);
                    if (match != null)
                    {
                        return match;
                    }

                    removename = spellmessage.IndexOf(" ");
                    if (removename != -1)
                    {
                        var nextfirst = firstpart + spellmessage.Substring(0, removename) + " ";
                        spellmessage = spellmessage.Substring(removename).Trim();
                        match = Match(spellmessage, nextfirst);
                        if (match != null)
                        {
                            return match;
                        }
                    }
                }
            }
            return null;
        }

        private SpellParsingMatch Match(string message, string removedpart)
        {
            Debug.WriteLine($"Match '{removedpart}'");
            var removename = message.IndexOf(" ");
            if (removename != -1)
            {
                var spellmessage = message.Substring(removename).Trim();
                var foundspells = new List<Spell>();

                if (spells.CastOtherSpells.TryGetValue(spellmessage, out foundspells))
                {
                    foundspells = foundspells.Where(a => !IgnoreSpellsForGuesses.Contains(a.name)).ToList();
                    var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                    var targetname = removedpart + message.Replace(foundspell.cast_on_other, string.Empty).Trim();
                    Debug.WriteLine($"Other Spell: {foundspell.name} Message: {spellmessage}");
                    var multiplematches = foundspell.Classes.All(a => a.Value == 255) && foundspells.Count > 1;
                    return new SpellParsingMatch
                    {
                        Spell = foundspell,
                        TargetName = targetname,
                        MultipleMatchesFound = multiplematches
                    };
                }

                if (spells.CastOnYouSpells.TryGetValue(message, out foundspells))
                {
                    foundspells = foundspells.Where(a => !IgnoreSpellsForGuesses.Contains(a.name)).ToList();
                    var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                    var targetname = removedpart + message.Replace(foundspell.cast_on_other, string.Empty).Trim();
                    Debug.WriteLine($"Cast On you Spell: {foundspell.name} Message: {spellmessage}");
                    var multiplematches = foundspell.Classes.All(a => a.Value == 255) && foundspells.Count > 1;
                    return new SpellParsingMatch
                    {
                        Spell = foundspell,
                        TargetName = EQSpells.SpaceYou,
                        MultipleMatchesFound = multiplematches
                    };
                }
            }

            return null;
        }
    }
}
