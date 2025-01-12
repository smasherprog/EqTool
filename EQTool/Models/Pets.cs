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
    // basic data structure (where ___o indicates has-many, ___. indicates has-one):
    //
    // [Pets] ____. [PLayerPet]
    //        \___o [PetSpell] ___o [PetRank]
    //
    //      Pets:       Contains in instance of the PlayerPet, representing the actual in-game pet
    //                  Contains a Dictionary of all PetSpell objects, where key = spell name, value = associated PetSpell object
    //                  Global list, loaded by DI
    //      PlayerPet:  Represents the actual in-game pet
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
        //private SettingsWindowViewModel viewModel;
        private Pets        pets;

        private PetSpell    petSpell = null;
        private string      petName = "";

        private int         maxObservedMelee = 0;
        private int         rankIndex = -1;

        // ctor
        //public PlayerPet(EQSpells spells, SettingsWindowViewModel vm)
        public PlayerPet(EQSpells spells)
        {
            pets = new Pets(spells);
            //viewModel = vm;

            // just for testing
            //PetSpell p = pets.PetSpellDictionary["Emissary of Thule"];
            //PetSpell p = pets.PetSpellDictionary["Minion of Shadows"];
            //PetSpell p = pets.PetSpellDictionary["Leering Corpse"];
            //PetSpell = p;
            //PetName = "Bakalakadaka";
            //rankIndex = 1;
            //viewModel.PetViewModel.HighLightRow(rankIndex);
        }

        public Pets Pets { get { return pets; } }

        // reset pet data
        public void Reset()
        {
            petSpell = null;
            petName = "";

            maxObservedMelee = 0;
            rankIndex = -1;

            // tell VM
            //viewModel.PetViewModel.Reset();
        }

        // get/set the PetSpell
        public PetSpell PetSpell 
        { 
            get { return petSpell; } 
            set 
            { 
                Reset();
                petSpell = value;

                // tell VM
                //viewModel.PetViewModel.SetPetSpell(petSpell);
            }
        }

        // is the pet active in game?
        public bool IsActive { get { return (petSpell != null); } }

        // get/set the Pet Name
        public string PetName 
        { 
            get { return petName; }
            set 
            { 
                petName = value;

                // tell VM
                //viewModel.PetViewModel.SetPetName(petName);
            }
        }

        // is pet name known?
        public bool IsPetNameKnown { get { return (petName != ""); } }

        // check for a new max melee, and/or a new rank
        public void CheckMaxMelee(int damage)
        {
            if (IsActive)
            {
                // new high?
                if (damage > maxObservedMelee)
                {
                    maxObservedMelee = damage;

                    // walk the list of ranks and see if this matches a rank
                    for (int ndx = 0; ndx < petSpell.PetRankList.Count; ndx++)
                    {
                        PetRank petRank = petSpell.PetRankList[ndx];
                        if (damage == petRank.MaxMelee)
                        {
                            rankIndex = ndx;

                            // tell VM
                            //viewModel.PetViewModel.HighLightRow(rankIndex);
                        }
                    }
                }
            }
        }

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
                        int lifetapProc = 0,
                        int damageShield = 0,
                        string description = "")
        {
            Rank = rank;
            PetLevel = petLevel;
            MaxMelee = maxMelee;
            MaxBashKick = maxBashKick;
            MaxBackstab = maxBackstab;
            LifetapProc = lifetapProc;
            DamageShield = damageShield;
            Description = description;
        }

        public string   Rank { get; }
        public int      PetLevel { get; }
        public int      MaxMelee { get; }
        public int      MaxBashKick { get; }
        public int      MaxBackstab { get; }
        public int      LifetapProc { get; }
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

            MageType = "";

            // create an empty list, ready to be populated
            PetRankList = new List<PetRank>();
        }

        public string                           SpellName { get; }
        public Dictionary<PlayerClasses, int>   Classes { get; }
        public List<Tuple<PetReagent, int>>     PetReagents { get; }
        public List<PetRank>                    PetRankList { get; }

        // todo - I'm not sure this field is needed?
        // use enum instead?
        public string                           MageType { get; }           

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
                    // the spells must be loaded before we can load the pets, since we extract some info from the spells data structure
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
            // necro pets
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
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 40, maxMelee: 49, maxBackstab: 147, lifetapProc: 40));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 41, maxMelee: 51, maxBackstab: 153, lifetapProc: 41));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 42, maxMelee: 52, maxBackstab: 159, lifetapProc: 42));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 43, maxMelee: 55, maxBackstab: 165, lifetapProc: 43));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 44, maxMelee: 56, maxBackstab: 171, lifetapProc: 44, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 45, maxMelee: 59, maxBackstab: 177, lifetapProc: 45, description: "Max+Focus"));
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
            petSpell.PetRankList.Add(new PetRank(rank: "1/5", petLevel: 43, maxMelee: 52, maxBashKick: 24, lifetapProc: 44));
            petSpell.PetRankList.Add(new PetRank(rank: "2/5", petLevel: 44, maxMelee: 55, maxBashKick: 24, lifetapProc: 45));
            petSpell.PetRankList.Add(new PetRank(rank: "3/5", petLevel: 45, maxMelee: 56, maxBashKick: 25, lifetapProc: 46));
            petSpell.PetRankList.Add(new PetRank(rank: "4/5", petLevel: 46, maxMelee: 59, maxBashKick: 25, lifetapProc: 47));
            petSpell.PetRankList.Add(new PetRank(rank: "5/5", petLevel: 47, maxMelee: 61, maxBashKick: 26, lifetapProc: 48, description: "Max"));
            petSpell.PetRankList.Add(new PetRank(rank: "6/5", petLevel: 48, maxMelee: 62, maxBashKick: 26, lifetapProc: 49, description: "Max+Focus"));
            _PetSpellDictionary[petSpell.SpellName] = petSpell;

            // 
            // Shaman pets
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
            // todo - Enchanter pets
            //

            // 
            // todo - Generic charmed pet
            //

            // 
            // todo - Mage pets
            //

        }


    }
}
