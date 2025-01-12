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
        // max number of pet ranks = 6 (5x normal ranks, 1x focused rank)
        private const int RankRowsCount = 6;

        // ctor
        public PetViewModel()
        {
            // initialize the row colors
            for (int ndx = 0; ndx < RankRowsCount; ndx++)
            {
                _RowColor.Add(new System.Windows.Media.SolidColorBrush());
            }
            ResetRowBackgrounds();
        }

        // name of the pet
        private string _PetName = "";
        public string PetName 
        { 
            get { return _PetName; }
            set { _PetName = value; OnPropertyChanged(); }
        }

        // the PetSpell for this spell and for display
        private PetSpell _PetSpell = null;
        public PetSpell PetSpell
        {
            get { return _PetSpell; }
            set
            {
                // set the petspell
                _PetSpell = value;

                // set the derived fields
                if (PetSpell != null)
                {
                    SpellName = PetSpell.SpellName;
                    Classes = PetSpell.Classes;
                    PetReagents = PetSpell.PetReagents;
                    PetRankList = PetSpell.PetRankList;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PetSpell.Classes));
                OnPropertyChanged(nameof(PetSpell.PetReagents));
                OnPropertyChanged(nameof(PetSpell.PetRankList));
            }
        }


        // clear all fields
        public void Reset()
        {
            PetName = "";
            PetSpell = null;
            SpellName = "";

            Classes.Clear();
            OnPropertyChanged(nameof(Classes));
            ClassNames = "";
            ClassLevels = "";

            PetReagents.Clear();
            OnPropertyChanged(nameof(PetReagents));
            PetReagentsText = "";

            PetRankList.Clear();
            OnPropertyChanged(nameof(PetRankList));

            ResetRowBackgrounds();
        }

        // spell name
        private string _SpellName = "";
        public string SpellName
        {
            get { return _SpellName; } 
            set { _SpellName = value; OnPropertyChanged(); }
        }

        // dictionary of (classes, required levels) for this spell
        // key = PlayerClasses enu, value = level require
        private Dictionary<PlayerClasses, int> _Classes = new Dictionary<PlayerClasses, int>();
        public Dictionary<PlayerClasses, int> Classes 
        { 
            get { return _Classes; }
            set
            {
                // set the field value
                _Classes = value;

                // set the dervied ClassNames string property
                string classNames = "";
                int ndx = 0;
                foreach (PlayerClasses playerClasses in _Classes.Keys)
                {
                    classNames += playerClasses.ToString();
                    if (++ndx < _Classes.Keys.Count)
                    {
                        classNames += ", ";
                    }
                }
                ClassNames = classNames;

                // set the derived ClassLevels string property
                string classLevels = "";
                ndx = 0;
                foreach (int level in _Classes.Values)
                {
                    classLevels += $"{level}";
                    if (++ndx < _Classes.Values.Count)
                    {
                        classLevels += ", ";
                    }
                }
                ClassLevels = classLevels;
                OnPropertyChanged();
            }
        }

        private string _ClassNames = "";
        public string ClassNames
        {
            get { return _ClassNames; }
            set { _ClassNames = value; OnPropertyChanged(); }
        }

        private string _ClassLevels = "";
        public string ClassLevels
        {
            get { return _ClassLevels; }
            set { _ClassLevels = value; OnPropertyChanged(); }
        }


        // reagents, kept in a list of pairs of (PetReagent enum, number of that reagent required)
        private List<Tuple<PetReagent, int>> _PetReagents = new List<Tuple<PetReagent, int>>();
        public List<Tuple<PetReagent, int>> PetReagents
        {
            get { return _PetReagents; }
            set
            {
                // set the field value
                _PetReagents = value;

                // set the dervied string property
                string reagentText = "";
                int ndx = 0;
                foreach (Tuple<PetReagent, int> pair in _PetReagents)
                {
                    reagentText += $"{pair.Item2}x {pair.Item1}";
                    if (++ndx < _PetReagents.Count)
                    {
                        reagentText += ", ";
                    }
                }
                PetReagentsText = reagentText;
                OnPropertyChanged();
            }
        }

        private string _PetReagentsText = "";
        public string PetReagentsText
        {
            get { return _PetReagentsText; }
            set { _PetReagentsText = value; OnPropertyChanged(); }
        }

        // list of PetRank objects
        private List<PetRank> _PetRankList = new List<PetRank>();
        public List<PetRank> PetRankList 
        { 
            get { return _PetRankList; } 
            set { _PetRankList = value; OnPropertyChanged(); }
        }


        // data to support dynamic highlighting of any given row
        private readonly System.Windows.Media.Brush _HighLightColor = System.Windows.Media.Brushes.LightGreen;
        private readonly System.Windows.Media.Brush _NormalColor = System.Windows.Media.Brushes.White;

        // list of colors, 1 per row
        private readonly List<System.Windows.Media.Brush> _RowColor = new List<System.Windows.Media.Brush>();
        public List<System.Windows.Media.Brush> RowColor { get { return _RowColor; } }

        // un-highlight all row colors
        private void ResetRowBackgrounds()
        {
            for (int ndx = 0; ndx < RankRowsCount; ndx++)
            {
                _RowColor[ndx] = _NormalColor;
                //_RowColor[ndx] = _HighLightColor;

                // force the changed property to match what the XAML field is binding to
                OnPropertyChanged(nameof(RowColor));
            }
        }

        // highlight a row to indicate which rank this particular pet is
        public void HighLightRow(int ndx)
        {
            if ((ndx >= 0) && (ndx < RankRowsCount))
            {
                ResetRowBackgrounds();
                _RowColor[ndx] = _HighLightColor;

                // force the changed property to match what the XAML field is binding to
                OnPropertyChanged(nameof(RowColor));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
