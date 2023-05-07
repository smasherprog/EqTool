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
            if (spells.CastOnYouSpells.TryGetValue(message, out var foundspells))
            {
                foundspells = foundspells.Where(a => !IgnoreSpellsForGuesses.Contains(a.name)).ToList();
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                Debug.WriteLine($"Cast On you Spell: {foundspell.name} Message: {message}");
                var multiplematches = foundspell.Classes.All(a => a.Value == 255) && foundspells.Count > 1;
                return new SpellParsingMatch
                {
                    Spell = foundspell,
                    TargetName = EQSpells.SpaceYou,
                    MultipleMatchesFound = multiplematches
                };
            }

            var removename = message.IndexOf("'");
            if (removename != -1)
            {
                var spellmessage = message.Substring(removename).Trim();
                if (spells.CastOtherSpells.TryGetValue(spellmessage, out foundspells))
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
                removename = 0;
                const int maxspaces = 5;
                for (var i = 0; i < maxspaces; i++)
                {
                    if (removename > message.Length)
                    {
                        break;
                    }
                    removename = message.IndexOf(" ", removename + 1);
                    if (removename != -1)
                    {
                        var firstpart = message.Substring(0, removename + 1).Trim();
                        var spellmessage = message.Substring(removename).Trim();
                        var match = Match(spellmessage, firstpart);
                        if (match != null)
                        {
                            return match;
                        }
                    }
                }
            }
            return null;
        }

        private SpellParsingMatch Match(string spellmessage, string targetname)
        {

            var foundspells = new List<Spell>();

            if (spells.CastOtherSpells.TryGetValue(spellmessage, out foundspells))
            {
                foundspells = foundspells.Where(a => !IgnoreSpellsForGuesses.Contains(a.name)).ToList();
                var foundspell = SpellDurations.MatchClosestLevelToSpell(foundspells, activePlayer.Player);
                Debug.WriteLine($"Other Spell: {foundspell.name} Message: {spellmessage}");
                var multiplematches = foundspell.Classes.All(a => a.Value == 255) && foundspells.Count > 1;
                return new SpellParsingMatch
                {
                    Spell = foundspell,
                    TargetName = targetname,
                    MultipleMatchesFound = multiplematches
                };
            }

            return null;
        }
    }
}
