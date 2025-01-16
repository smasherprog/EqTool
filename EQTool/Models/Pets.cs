using Autofac;
using System;
using EQTool.Models;
using EQTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using EQToolShared.Enums;
using System.Configuration;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace EQTool.Models
{
    //
    // basic data structure (where ---o indicates has-many, ---. indicates has-one):
    //
    // [PLayerPet] ---. [Pets] ---o [PetSpell] ---o [PetRank]
    // 
    //      PlayerPet:  Represents the actual in-game pet
    //                  Global, loaded by DI
    //      Pets:       Contains a Dictionary of all PetSpell objects, where key = spell name, value = associated PetSpell object
    //      PetSpell:   One object for each different pet spell
    //                  Contains a List of PetRank objects
    //      PetRank:    Has the relevant stats (pet level, max melee, and so on) for that rank of that PetSpell
    //

    // ===============================================================================================================

    //
    // class to represent the actual in-game pet
    //
    public class PlayerPet
    {
        private readonly PetViewModel petViewModel;


        private int maxObservedMelee = 0;

        // ctor
        public PlayerPet(EQSpells spells, PetViewModel vm)
        {
            this.petViewModel = vm;

            // create the container of all the PetSpell objects
            this._Pets = new Pets(spells);
        }

        // reset PlayerPet data
        public void Reset()
        {
            _PetSpell = null;
            _PetName = "";

            maxObservedMelee = 0;
            rankIndex = -1;

            // tell VM
            petViewModel.Reset();
        }

        // container of all PetSpell objects
        private readonly Pets _Pets;
        public Pets Pets
        {
            get
            {
                return _Pets;
            }
        }

        // get/set the PetSpell
        private PetSpell _PetSpell = null;
        public PetSpell PetSpell 
        { 
            get { return _PetSpell; } 
            set 
            { 
                Reset();
                _PetSpell = value;

                // tell VM
                petViewModel.PetSpell = value;
            }
        }

        // is the pet active in game?
        public bool IsActive { get { return (_PetSpell != null); } }

        // get/set the Pet Name
        private string _PetName = "";
        public string PetName 
        { 
            get { return _PetName; }
            set 
            { 
                _PetName = value;

                // tell VM
                petViewModel.PetName = value;
            }
        }

        // is pet name known?
        public bool IsPetNameKnown { get { return (_PetName != ""); } }

        // check for a new max melee, and/or a new rank
        public void CheckMaxMelee(int damage)
        {
            if (IsActive && IsPetNameKnown)
            {
                // new high?
                if (damage > maxObservedMelee)
                {
                    maxObservedMelee = damage;

                    // walk the list of ranks and see if this matches a rank
                    for (int ndx = 0; ndx < _PetSpell.PetRankList.Count; ndx++)
                    {
                        PetRank petRank = _PetSpell.PetRankList[ndx];
                        if (damage == petRank.MaxMelee)
                        {
                            rankIndex = ndx;

                            // tell VM
                            petViewModel.HighLightRow(rankIndex);
                        }
                    }
                }
            }
        }

        private int rankIndex = -1;
        public int RankIndex { get { return rankIndex; } }

    }

    // ===============================================================================================================

    //
    // class to represent a single pet rank, ndx.e. the stats for the max pet, or min pet, and all inbetween
    //
    public class PetRank
    {
        // ctor
        public PetRank(string rank,
                        int petLevel,
                        int maxMelee,
                        int maxBashKick = 0,
                        int maxBackstab = 0,
                        string lifetapOrProc = "",
                        int damageShield = 0,
                        string description = "")
        {
            Rank = rank;
            PetLevel = petLevel;
            MaxMelee = maxMelee;
            MaxBashKick = maxBashKick;
            MaxBackstab = maxBackstab;
            LifetapOrProc = lifetapOrProc;
            DamageShield = damageShield;
            Description = description;
        }

        public string   Rank { get; }
        public int      PetLevel { get; }
        public int      MaxMelee { get; }
        public int      MaxBashKick { get; }
        public int      MaxBackstab { get; }
        public string   LifetapOrProc { get; }
        public int      DamageShield { get; }
        public string   Description { get; }

    }

    // ===============================================================================================================

    //
    // class to represent a Pet Spell, such as "Leering Corpse", "Emissary of Thule", etc
    // note that this class has a list of all the possible PetRanks for this spell
    //
    public class PetSpell
    {
        // ctor
        public PetSpell(string spellName, EQSpells spells)
        {
            SpellName = spellName;

            // extract these data from the already-parsed spells file
            Spell spell = spells.AllSpells.FirstOrDefault(a => a.name == spellName);
            Classes = spell.Classes;
            PetReagents = spell.PetReagents;

            // create an empty list, ready to be populated
            PetRankList = new List<PetRank>();
        }

        // a kind-of copy ctor - for use with the Mage pet near-clones situation
        public PetSpell(string spellName, PetSpell source)
        {
            SpellName = spellName;
            Classes = source.Classes;
            PetReagents = source.PetReagents;
            PetRankList = source.PetRankList;
        }

        public string                           SpellName { get; }
        public Dictionary<PlayerClasses, int>   Classes { get; }
        public List<Tuple<PetReagent, int>>     PetReagents { get; }
        public List<PetRank>                    PetRankList { get; }

    }



    // ===============================================================================================================

    //
    // class to serve as a container for all PetSpell objects
    //
    public class Pets
    {
        // reference to DI globals
        private readonly EQSpells eqSpells;

        // ctor
        public Pets(EQSpells eqSpells)
        {
            this.eqSpells = eqSpells;
        }

        // returns dictionary of PetSpell objects, key = spell name, value = corresponding PetSpell object
        private readonly Dictionary<string, PetSpell> _PetSpellDictionary = new Dictionary<string, PetSpell>();
        public Dictionary<string, PetSpell> PetSpellDictionary
        {
            get
            {
                // if the dictionary is empty...
                if (_PetSpellDictionary.Any() == false)
                {
                    // the spells must be loaded before we can load the _Pets, since we extract some info from the spells data structure
                    if (eqSpells.AllSpells.Any() == true)
                    {
                        LoadPetSpells();
                    }
                }
                return _PetSpellDictionary;
            }
        }

        // load all the PetSpell data
        private void LoadPetSpells()
        {
            //
            // necro Pets
            //
            PetSpell petSpell = new PetSpell(spellName: "Cavorting Bones", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/2", petLevel: 1, maxMelee: 8, maxBashKick: 0));
            petSpell.PetRankList.Add(new PetRank(rank: "2/2", petLevel: 2, maxMelee: 10, maxBashKick: 0));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Leering Corpse", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/3", petLevel: 3, maxMelee: 8, maxBashKick: 0));
            petSpell.PetRankList.Add(new PetRank(rank: "2/3", petLevel: 4, maxMelee: 10, maxBashKick: 0));
            petSpell.PetRankList.Add(new PetRank(rank: "3/3", petLevel: 5, maxMelee: 12, maxBashKick: 0, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Bone Walk", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/4", petLevel: 6, maxMelee: 8, maxBashKick: 8));
            petSpell.PetRankList.Add(new PetRank(rank: "2/4", petLevel: 7, maxMelee: 10, maxBashKick: 10));
            petSpell.PetRankList.Add(new PetRank(rank: "3/4", petLevel: 8, maxMelee: 12, maxBashKick: 12));
            petSpell.PetRankList.Add(new PetRank(rank: "4/4", petLevel: 9, maxMelee: 14, maxBashKick: 13, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Convoke Shadow", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/4", petLevel: 8, maxMelee: 10, maxBashKick: 10));
            petSpell.PetRankList.Add(new PetRank(rank: "2/4", petLevel: 9, maxMelee: 12, maxBashKick: 12));
            petSpell.PetRankList.Add(new PetRank(rank: "3/4", petLevel: 10, maxMelee: 14, maxBashKick: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "4/4", petLevel: 11, maxMelee: 16, maxBashKick: 16, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Restless Bones", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 12, maxMelee: 12, maxBashKick: 12));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 13, maxMelee: 14, maxBashKick: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 14, maxMelee: 16, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 15, maxMelee: 18, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 16, maxMelee: 20, maxBashKick: 16, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Animate Dead", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 15, maxMelee: 14, maxBashKick: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 16, maxMelee: 16, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 17, maxMelee: 18, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 18, maxMelee: 20, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 19, maxMelee: 22, maxBashKick: 16, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Haunting Corpse", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 18, maxMelee: 18, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 19, maxMelee: 20, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 20, maxMelee: 22, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 21, maxMelee: 23, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 22, maxMelee: 26, maxBashKick: 17, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Summon Dead", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 22, maxMelee: 20, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 23, maxMelee: 22, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 24, maxMelee: 23, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 25, maxMelee: 26, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 26, maxMelee: 28, maxBashKick: 18, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Invoke Shadow", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 25, maxMelee: 23, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 26, maxMelee: 26, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 27, maxMelee: 28, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 28, maxMelee: 30, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 29, maxMelee: 32, maxBashKick: 19, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Malignant Dead", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 29, maxMelee: 31, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 30, maxMelee: 33, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 31, maxMelee: 35, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 32, maxMelee: 37, maxBashKick: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 33, maxMelee: 39, maxBashKick: 20, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Cackling Bones", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 33, maxMelee: 39, maxBashKick: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 34, maxMelee: 41, maxBashKick: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 35, maxMelee: 43, maxBashKick: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 36, maxMelee: 45, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 37, maxMelee: 47, maxBashKick: 22, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Invoke Death", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 37, maxMelee: 47, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 38, maxMelee: 49, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 39, maxMelee: 51, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 40, maxMelee: 52, maxBashKick: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 41, maxMelee: 55, maxBashKick: 24, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 42, maxMelee: 57, maxBashKick: 25, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Minion of Shadows", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 40, maxMelee: 49, maxBackstab: 147, lifetapOrProc: "40"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 41, maxMelee: 51, maxBackstab: 153, lifetapOrProc: "41"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 42, maxMelee: 52, maxBackstab: 159, lifetapOrProc: "42"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 43, maxMelee: 55, maxBackstab: 165, lifetapOrProc: "43"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 44, maxMelee: 56, maxBackstab: 171, lifetapOrProc: "44", description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 45, maxMelee: 59, maxBackstab: 177, lifetapOrProc: "45", description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Servant of Bones", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 40, maxMelee: 51, maxBashKick: 63));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 41, maxMelee: 52, maxBashKick: 65));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 42, maxMelee: 55, maxBashKick: 66));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 43, maxMelee: 56, maxBashKick: 68));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 44, maxMelee: 59, maxBashKick: 69, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 45, maxMelee: 61, maxBashKick: 71, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Emissary of Thule", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 43, maxMelee: 52, maxBashKick: 24, lifetapOrProc: "44"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 44, maxMelee: 55, maxBashKick: 24, lifetapOrProc: "45"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 45, maxMelee: 56, maxBashKick: 25, lifetapOrProc: "46"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 46, maxMelee: 59, maxBashKick: 25, lifetapOrProc: "47"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 47, maxMelee: 61, maxBashKick: 26, lifetapOrProc: "48", description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 48, maxMelee: 62, maxBashKick: 26, lifetapOrProc: "49", description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // 
            // Shaman Pets
            //
            petSpell = new PetSpell(spellName: "Companion Spirit", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 22, maxMelee: 22, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 23, maxMelee: 23, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 24, maxMelee: 26, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 25, maxMelee: 28, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 26, maxMelee: 30, maxBashKick: 18, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Vigilant Spirit", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 24, maxMelee: 27, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 25, maxMelee: 28, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 26, maxMelee: 31, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 27, maxMelee: 33, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 28, maxMelee: 35, maxBashKick: 19, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Guardian Spirit", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 28, maxMelee: 35, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 29, maxMelee: 37, maxBashKick: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 30, maxMelee: 39, maxBashKick: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 31, maxMelee: 41, maxBashKick: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 32, maxMelee: 43, maxBashKick: 21, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Frenzied Spirit", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 32, maxMelee: 43, maxBashKick: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 33, maxMelee: 45, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 34, maxMelee: 47, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 35, maxMelee: 49, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 36, maxMelee: 51, maxBashKick: 23, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Spirit of the Howler", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 35, maxMelee: 45, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 36, maxMelee: 47, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 37, maxMelee: 49, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 38, maxMelee: 51, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 39, maxMelee: 52, maxBashKick: 24, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // 
            // Enchanter Pets
            //
            petSpell = new PetSpell(spellName: "Pendril's Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/2", petLevel: 1, maxMelee: 7, maxBashKick: 0));
            petSpell.PetRankList.Add(new PetRank(rank: "2/2", petLevel: 2, maxMelee: 9, maxBashKick: 0, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Juli`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/3", petLevel: 3, maxMelee: 9, maxBashKick: 0));
            petSpell.PetRankList.Add(new PetRank(rank: "2/3", petLevel: 4, maxMelee: 10, maxBashKick: 0));
            petSpell.PetRankList.Add(new PetRank(rank: "3/3", petLevel: 5, maxMelee: 12, maxBashKick: 0, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Mircyl's Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/4", petLevel: 6, maxMelee: 9, maxBashKick: 8));
            petSpell.PetRankList.Add(new PetRank(rank: "2/4", petLevel: 7, maxMelee: 10, maxBashKick: 10));
            petSpell.PetRankList.Add(new PetRank(rank: "3/4", petLevel: 8, maxMelee: 12, maxBashKick: 12));
            petSpell.PetRankList.Add(new PetRank(rank: "4/4", petLevel: 9, maxMelee: 14, maxBashKick: 13, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Kilan`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/4", petLevel: 9, maxMelee: 11, maxBashKick: 11));
            petSpell.PetRankList.Add(new PetRank(rank: "2/4", petLevel: 10, maxMelee: 13, maxBashKick: 13));
            petSpell.PetRankList.Add(new PetRank(rank: "3/4", petLevel: 11, maxMelee: 15, maxBashKick: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "4/4", petLevel: 12, maxMelee: 17, maxBashKick: 15, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Shalee`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 12, maxMelee: 12, maxBashKick: 12));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 13, maxMelee: 14, maxBashKick: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 14, maxMelee: 16, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 15, maxMelee: 18, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 16, maxMelee: 20, maxBashKick: 16, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Sisna`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 16, maxMelee: 14, maxBashKick: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 17, maxMelee: 16, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 18, maxMelee: 18, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 19, maxMelee: 20, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 20, maxMelee: 22, maxBashKick: 16, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Sagar`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 19, maxMelee: 18, maxBashKick: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 20, maxMelee: 20, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 21, maxMelee: 22, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 22, maxMelee: 23, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 23, maxMelee: 26, maxBashKick: 17, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Uleen`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 22, maxMelee: 20, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 23, maxMelee: 22, maxBashKick: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 24, maxMelee: 23, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 25, maxMelee: 26, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 26, maxMelee: 28, maxBashKick: 18, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Boltran`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 25, maxMelee: 26, maxBashKick: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 26, maxMelee: 28, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 27, maxMelee: 30, maxBashKick: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 28, maxMelee: 32, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 29, maxMelee: 34, maxBashKick: 19, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Aanya's Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 29, maxMelee: 32, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 30, maxMelee: 34, maxBashKick: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 31, maxMelee: 36, maxBashKick: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 32, maxMelee: 38, maxBashKick: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 33, maxMelee: 40, maxBashKick: 21, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Yegoreff`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 33, maxMelee: 40, maxBashKick: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 34, maxMelee: 42, maxBashKick: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 35, maxMelee: 44, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 36, maxMelee: 45, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 37, maxMelee: 48, maxBashKick: 23, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Kintaz`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 37, maxMelee: 45, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 38, maxMelee: 47, maxBashKick: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 39, maxMelee: 49, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 40, maxMelee: 51, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 41, maxMelee: 52, maxBashKick: 24, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell(spellName: "Zumaik`s Animation", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 44, maxMelee: 49, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 45, maxMelee: 51, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 46, maxMelee: 52, maxBashKick: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 47, maxMelee: 55, maxBashKick: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 48, maxMelee: 56, maxBashKick: 25, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // 
            // Mage Pets
            //
            // note we are taking advantage of the fact that the Air/Earth/Fire/Water pet spells all have same stats for their ranks,
            // so we add the PetRank objects to the first one, then use the special copy ctor to clone the others
            //

            // level 4
            string baseSpellName = "Elementalkin";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/3", petLevel: 4, maxMelee: 8, maxBashKick: 0, lifetapOrProc: "5", damageShield: 6));
            petSpell.PetRankList.Add(new PetRank(rank: "2/3", petLevel: 5, maxMelee: 10, maxBashKick: 0, lifetapOrProc: "6", damageShield: 7));
            petSpell.PetRankList.Add(new PetRank(rank: "3/3", petLevel: 6, maxMelee: 12, maxBashKick: 10, lifetapOrProc: "7", damageShield: 8, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 8
            baseSpellName = "Elementaling";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/4", petLevel: 6, maxMelee: 10, maxBashKick: 9, lifetapOrProc: "7", damageShield: 8));
            petSpell.PetRankList.Add(new PetRank(rank: "2/4", petLevel: 7, maxMelee: 12, maxBashKick: 11, lifetapOrProc: "8", damageShield: 9));
            petSpell.PetRankList.Add(new PetRank(rank: "3/4", petLevel: 8, maxMelee: 14, maxBashKick: 12, lifetapOrProc: "9", damageShield: 10));
            petSpell.PetRankList.Add(new PetRank(rank: "4/4", petLevel: 9, maxMelee: 16, maxBashKick: 14, lifetapOrProc: "10", damageShield: 11, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 12
            baseSpellName = "Elemental";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/4", petLevel: 10, maxMelee: 12, maxBashKick: 12, lifetapOrProc: "11", damageShield: 12));
            petSpell.PetRankList.Add(new PetRank(rank: "2/4", petLevel: 11, maxMelee: 14, maxBashKick: 14, lifetapOrProc: "12", damageShield: 13));
            petSpell.PetRankList.Add(new PetRank(rank: "3/4", petLevel: 12, maxMelee: 16, maxBashKick: 15, lifetapOrProc: "13", damageShield: 14));
            petSpell.PetRankList.Add(new PetRank(rank: "4/4", petLevel: 13, maxMelee: 18, maxBashKick: 15, lifetapOrProc: "14", damageShield: 15, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 16
            baseSpellName = "Minor Summoning";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 10, maxMelee: 12, maxBashKick: 12, lifetapOrProc: "14", damageShield: 15));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 11, maxMelee: 14, maxBashKick: 14, lifetapOrProc: "15", damageShield: 16));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 12, maxMelee: 16, maxBashKick: 15, lifetapOrProc: "16", damageShield: 17));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 12, maxMelee: 18, maxBashKick: 15, lifetapOrProc: "17", damageShield: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 13, maxMelee: 20, maxBashKick: 16, lifetapOrProc: "18", damageShield: 19, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 20
            baseSpellName = "Lesser Summoning";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 15, maxMelee: 14, maxBashKick: 14, lifetapOrProc: "17", damageShield: 18));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 16, maxMelee: 16, maxBashKick: 15, lifetapOrProc: "18", damageShield: 19));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 17, maxMelee: 18, maxBashKick: 15, lifetapOrProc: "19", damageShield: 20));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 18, maxMelee: 20, maxBashKick: 16, lifetapOrProc: "20", damageShield: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 19, maxMelee: 22, maxBashKick: 16, lifetapOrProc: "21", damageShield: 22, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 24
            baseSpellName = "Summoning";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 19, maxMelee: 16, maxBashKick: 15, lifetapOrProc: "20", damageShield: 21));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 20, maxMelee: 18, maxBashKick: 15, lifetapOrProc: "21", damageShield: 22));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 21, maxMelee: 20, maxBashKick: 16, lifetapOrProc: "22", damageShield: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 22, maxMelee: 22, maxBashKick: 16, lifetapOrProc: "23", damageShield: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 23, maxMelee: 23, maxBashKick: 17, lifetapOrProc: "24", damageShield: 25, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 29
            baseSpellName = "Greater Summoning";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 22, maxMelee: 20, maxBashKick: 16, lifetapOrProc: "23", damageShield: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 23, maxMelee: 22, maxBashKick: 16, lifetapOrProc: "24", damageShield: 25));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 24, maxMelee: 23, maxBashKick: 17, lifetapOrProc: "25", damageShield: 26));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 25, maxMelee: 26, maxBashKick: 17, lifetapOrProc: "26", damageShield: 27));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 26, maxMelee: 28, maxBashKick: 18, lifetapOrProc: "27", damageShield: 28, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 34
            baseSpellName = "Minor Conjuration";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 25, maxMelee: 26, maxBashKick: 17, lifetapOrProc: "26", damageShield: 27));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 26, maxMelee: 28, maxBashKick: 18, lifetapOrProc: "27", damageShield: 28));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 27, maxMelee: 30, maxBashKick: 18, lifetapOrProc: "28", damageShield: 29));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 28, maxMelee: 32, maxBashKick: 19, lifetapOrProc: "29", damageShield: 30));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 29, maxMelee: 34, maxBashKick: 19, lifetapOrProc: "30", damageShield: 31, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 39
            baseSpellName = "Lesser Conjuration";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 29, maxMelee: 32, maxBashKick: 19, lifetapOrProc: "30", damageShield: 31));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 30, maxMelee: 34, maxBashKick: 19, lifetapOrProc: "31", damageShield: 32));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 31, maxMelee: 36, maxBashKick: 20, lifetapOrProc: "32", damageShield: 33));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 32, maxMelee: 38, maxBashKick: 20, lifetapOrProc: "33", damageShield: 34));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 33, maxMelee: 40, maxBashKick: 21, lifetapOrProc: "34", damageShield: 35, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 44 - todo guessed at bash/kick stats
            baseSpellName = "Conjuration";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 33, maxMelee: 40, maxBashKick: 21, lifetapOrProc: "34", damageShield: 35));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 34, maxMelee: 42, maxBashKick: 21, lifetapOrProc: "35", damageShield: 36));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 35, maxMelee: 44, maxBashKick: 22, lifetapOrProc: "36", damageShield: 37));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 36, maxMelee: 45, maxBashKick: 22, lifetapOrProc: "37", damageShield: 38));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 37, maxMelee: 48, maxBashKick: 23, lifetapOrProc: "38", damageShield: 39, description: "Max"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 49
            baseSpellName = "Greater Conjuration";
            petSpell = new PetSpell(spellName: $"{baseSpellName}: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 37, maxMelee: 48, maxBashKick: 23, lifetapOrProc: "38", damageShield: 39));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 38, maxMelee: 50, maxBashKick: 23, lifetapOrProc: "39", damageShield: 40));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 39, maxMelee: 52, maxBashKick: 24, lifetapOrProc: "40", damageShield: 41));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 40, maxMelee: 54, maxBashKick: 24, lifetapOrProc: "41", damageShield: 42));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 41, maxMelee: 56, maxBashKick: 25, lifetapOrProc: "42", damageShield: 43, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 42, maxMelee: 58, maxBashKick: 25, lifetapOrProc: "43", damageShield: 44, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            petSpell = new PetSpell($"{baseSpellName}: Earth", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Fire", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;
            petSpell = new PetSpell($"{baseSpellName}: Water", petSpell);
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 51 - todo guessed at all stats for the 6/5 rank
            petSpell = new PetSpell(spellName: "Vocarate: Earth", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 41, maxMelee: 50, maxBashKick: 23, lifetapOrProc: "51"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 42, maxMelee: 52, maxBashKick: 24, lifetapOrProc: "52"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 43, maxMelee: 54, maxBashKick: 24, lifetapOrProc: "53"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 44, maxMelee: 56, maxBashKick: 25, lifetapOrProc: "54"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 45, maxMelee: 58, maxBashKick: 25, lifetapOrProc: "55", description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 46, maxMelee: 60, maxBashKick: 26, lifetapOrProc: "56", description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 52 - todo guessed at max melee for the 6/5 rank
            petSpell = new PetSpell(spellName: "Vocarate: Fire", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 41, maxMelee: 21, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 42, maxMelee: 22, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 43, maxMelee: 25, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 44, maxMelee: 27, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 45, maxMelee: 28, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 46, maxMelee: 30, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 53 - todo guessed at all stats for the 6/5 rank
            petSpell = new PetSpell(spellName: "Vocarate: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 41, maxMelee: 48, maxBashKick: 63, lifetapOrProc: "51"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 42, maxMelee: 50, maxBashKick: 65, lifetapOrProc: "52"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 43, maxMelee: 52, maxBashKick: 67, lifetapOrProc: "53"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 44, maxMelee: 54, maxBashKick: 68, lifetapOrProc: "54"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 45, maxMelee: 56, maxBashKick: 70, lifetapOrProc: "55", description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 46, maxMelee: 58, maxBashKick: 72, lifetapOrProc: "56", description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 54 - todo guessed at all stats for the 6/5 rank
            petSpell = new PetSpell(spellName: "Vocarate: Water", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 41, maxMelee: 48, maxBashKick: 0, lifetapOrProc: "102", maxBackstab: 144));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 42, maxMelee: 50, maxBashKick: 0, lifetapOrProc: "104", maxBackstab: 150));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 43, maxMelee: 52, maxBashKick: 0, lifetapOrProc: "106", maxBackstab: 156));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 44, maxMelee: 54, maxBashKick: 0, lifetapOrProc: "108", maxBackstab: 162));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 45, maxMelee: 56, maxBashKick: 0, lifetapOrProc: "110", maxBackstab: 168, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 46, maxMelee: 58, maxBashKick: 0, lifetapOrProc: "112", maxBackstab: 174, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 57 - todo guessed at all data
            petSpell = new PetSpell(spellName: "Greater Vocaration: Earth", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 44, maxMelee: 52, maxBashKick: 26, lifetapOrProc: "54"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 45, maxMelee: 54, maxBashKick: 26, lifetapOrProc: "55"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 46, maxMelee: 56, maxBashKick: 27, lifetapOrProc: "56"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 47, maxMelee: 58, maxBashKick: 27, lifetapOrProc: "57"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 48, maxMelee: 60, maxBashKick: 28, lifetapOrProc: "58", description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 49, maxMelee: 62, maxBashKick: 28, lifetapOrProc: "59", description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 58 - todo guessed at all data
            petSpell = new PetSpell(spellName: "Greater Vocaration: Fire", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 44, maxMelee: 33, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 45, maxMelee: 34, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 46, maxMelee: 37, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 47, maxMelee: 39, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 48, maxMelee: 40, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 49, maxMelee: 42, maxBashKick: 0, lifetapOrProc: "83,110,179", damageShield: 2, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 59 - todo guessed at all data
            petSpell = new PetSpell(spellName: "Greater Vocaration: Air", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 44, maxMelee: 50, maxBashKick: 67, lifetapOrProc: "54"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 45, maxMelee: 52, maxBashKick: 68, lifetapOrProc: "55"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 46, maxMelee: 54, maxBashKick: 70, lifetapOrProc: "56"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 47, maxMelee: 56, maxBashKick: 72, lifetapOrProc: "57"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 48, maxMelee: 58, maxBashKick: 73, lifetapOrProc: "58", description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 49, maxMelee: 60, maxBashKick: 75, lifetapOrProc: "59", description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // level 60 - todo guessed at all data
            petSpell = new PetSpell(spellName: "Greater Vocaration: Water", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 44, maxMelee: 50, maxBashKick: 0, lifetapOrProc: "108", maxBackstab: 180));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 45, maxMelee: 52, maxBashKick: 0, lifetapOrProc: "110", maxBackstab: 186));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 46, maxMelee: 54, maxBashKick: 0, lifetapOrProc: "112", maxBackstab: 192));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 47, maxMelee: 56, maxBashKick: 0, lifetapOrProc: "114", maxBackstab: 198));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 48, maxMelee: 58, maxBashKick: 0, lifetapOrProc: "116", maxBackstab: 204, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 49, maxMelee: 60, maxBashKick: 0, lifetapOrProc: "118", maxBackstab: 210, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // Monster Summoning 1
            petSpell = new PetSpell("Monster Summoning I", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 25, maxMelee: 26, maxBashKick: 17, lifetapOrProc: "26"));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 26, maxMelee: 28, maxBashKick: 18, lifetapOrProc: "27"));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 27, maxMelee: 30, maxBashKick: 18, lifetapOrProc: "28"));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 28, maxMelee: 32, maxBashKick: 19, lifetapOrProc: "29"));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 29, maxMelee: 34, maxBashKick: 19, lifetapOrProc: "30"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // Monster Summoning 2
            // todo - does this spell really not have a proc or damage shield
            petSpell = new PetSpell("Monster Summoning II", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 37, maxMelee: 48, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 38, maxMelee: 50, maxBashKick: 23));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 39, maxMelee: 52, maxBashKick: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 40, maxMelee: 54, maxBashKick: 24));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 41, maxMelee: 56, maxBashKick: 25));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // todo - Monster Summoning 3


            // mage epic pet
            petSpell = new PetSpell(spellName: "Manifest Elements", spells: eqSpells);
            petSpell.PetRankList.Add(new PetRank(rank: "1/1", petLevel: 49, maxMelee: 67, maxBashKick: 27, lifetapOrProc: "143", damageShield: 50, description: "Epic Pet"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // 
            // todo - Generic charmed pet
            //


        }


    }
}
