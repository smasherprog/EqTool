using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EQTool.ViewModels
{
    // view model for Pet window.  readonly data that is displayed, cannot be edited by user
    public class PetViewModel : INotifyPropertyChanged
    {
        // global DI list of pets
        private readonly Pets pets;

        // the PetSpell for this spell and for display
        private PetSpell _PetSpell = null;

        // max number of pet ranks = 6 (5x normal ranks, 1x focused rank)
        private const int RankRowsCount = 6;

        // ctor
        public PetViewModel(Pets pets)
        {
            this.pets = pets;

            // initialize the row colors
            for (int ndx = 0; ndx < RankRowsCount; ndx++)
            {
                _RowColor.Add(new System.Windows.Media.SolidColorBrush());
            }
            ResetRowBackgrounds();

            // just for testing
            //PetSpell p = pets.PetSpellDictionary["Emissary of Thule"];
            //PetSpell p = pets.PetSpellDictionary["Minion of Shadows"];
            //PetSpell p = pets.PetSpellDictionary["Leering Corpse"];
            //SetPetSpell(p);
            //HighLightRow(1);
            //SetPetName("Bakalakadaka");
        }

        // set the pet name
        public void SetPetName(string petName)
        {
            _PetName = petName;
            OnPropertyChanged();
        }

        // set the pet spell, populate all fields
        public void SetPetSpell(PetSpell petSpell)
        {
            _PetSpell = petSpell;

            _SpellName = petSpell.SpellName;
            _Classes = petSpell.Classes;
            _PetReagents = petSpell.PetReagents;
            _PetRankList = petSpell.PetRankList;

            OnPropertyChanged();
        }

        // clear all fields
        public void Clear()
        {
            _PetSpell = null;
            _PetName = "";
            _SpellName = "";
            _Classes.Clear();
            _PetReagents.Clear();
            _PetRankList.Clear();
            ResetRowBackgrounds();

            OnPropertyChanged();
        }


        // name of the pet
        private string _PetName = "";
        public string PetName { get { return _PetName; } }

        // spell name
        private string _SpellName = "";
        public string SpellName { get { return _SpellName; } }

        // dictionary of (classes, required levels) for this spell
        // key = PlayerClasses enu, value = level require
        private Dictionary<PlayerClasses, int> _Classes = new Dictionary<PlayerClasses, int>();
        public string ClassNames
        {
            get
            {
                string rv = "";
                int ndx = 0;
                foreach (PlayerClasses playerClasses in _Classes.Keys)
                {
                    rv += playerClasses.ToString();
                    if (++ndx < _Classes.Keys.Count)
                    {
                        rv += ", ";
                    }
                }
                return rv;
            }
        }
        public string ClassLevels
        {
            get
            {
                // convert that data structure into a single readable string for display
                string rv = "";
                int ndx = 0;
                foreach (int level in _Classes.Values)
                {
                    rv += $"{level}";
                    if (++ndx < _Classes.Values.Count)
                    {
                        rv += ", ";
                    }
                }
                return rv;
            }
        }

        // reagents, kept in a list of pairs of (PetReagent enum, number of that reagent required)
        private List<Tuple<PetReagent, int>> _PetReagents = new List<Tuple<PetReagent, int>>();
        public string PetReagentsText
        {
            get
            {
                // convert that data structure into a single readable string for display
                string rv = "";
                int ndx = 0;
                foreach (Tuple<PetReagent, int> pair in _PetReagents)
                {
                    rv += $"{pair.Item2}x {pair.Item1}";
                    if (++ndx < _PetReagents.Count)
                    {
                        rv += ", ";
                    }
                }
                return rv;
            }
        }

        // list of PetRank objects
        private List<PetRank> _PetRankList = new List<PetRank>();
        public List<PetRank> PetRankList { get { return _PetRankList; } }

        // data to support dynamic highlighting of any given row
        private System.Windows.Media.Brush _HighLightColor = System.Windows.Media.Brushes.LightGreen;
        private System.Windows.Media.Brush _NormalColor = System.Windows.Media.Brushes.White;

        // list of colors, 1 per row
        private List<System.Windows.Media.Brush> _RowColor = new List<System.Windows.Media.Brush>();
        public List<System.Windows.Media.Brush> RowColor { get { return _RowColor; } }

        // un-highlight all row colors
        private void ResetRowBackgrounds()
        {
            for (int ndx = 0; ndx < RankRowsCount; ndx++)
            {
                _RowColor[ndx] = _NormalColor;
            }
        }

        // highlight a row to indicate which rank this particular pet is
        public void HighLightRow(int ndx)
        {
            if ((ndx >= 0) && (ndx < RankRowsCount))
            {
                ResetRowBackgrounds();
                _RowColor[ndx] = _HighLightColor;
            }
            OnPropertyChanged();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
