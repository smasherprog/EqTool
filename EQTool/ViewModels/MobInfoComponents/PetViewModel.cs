using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels.MobInfoComponents
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
            for (var ndx = 0; ndx < RankRowsCount; ndx++)
            {
                RowColor.Add(new System.Windows.Media.SolidColorBrush());
            }
            ResetRowBackgrounds();
        }

        // name of the pet
        private string _PetName = "";
        public string PetName
        {
            get => _PetName;
            set { _PetName = value; OnPropertyChanged(); }
        }

        // the PetSpell for this spell and for display
        private PetSpell _PetSpell = null;
        public PetSpell PetSpell
        {
            get => _PetSpell;
            set
            {
                // set the petspell
                _PetSpell = value;

                // set the derived fields
                if (_PetSpell != null)
                {
                    SpellName = PetSpell.SpellName;
                    Classes = PetSpell.Classes;
                    PetReagents = PetSpell.PetReagents;
                    PetRankList = PetSpell.PetRankList;
                }

                OnPropertyChanged();
            }
        }


        // clear all fields
        public void Reset()
        {
            PetName = "";
            PetSpell = null;
            SpellName = "";
            RankIndex = -1;

            Classes = null;
            PetReagents = null;
            PetRankList = null;

            ResetRowBackgrounds();
        }

        // spell name
        private string _SpellName = "";
        public string SpellName
        {
            get => _SpellName;
            set { _SpellName = value; OnPropertyChanged(); }
        }

        // dictionary of (classes, required levels) for this spell
        // key = PlayerClasses enu, value = level require
        private Dictionary<PlayerClasses, int> _Classes = new Dictionary<PlayerClasses, int>();
        public Dictionary<PlayerClasses, int> Classes
        {
            get => _Classes;
            set
            {
                // set the field value
                _Classes = value;
                ClassNames = "";
                ClassLevels = "";

                // set the dervied ClassNames string property
                if (_Classes != null)
                {
                    var classNames = "";
                    var ndx = 0;
                    foreach (var playerClasses in _Classes.Keys)
                    {
                        classNames += $"{playerClasses}";
                        if (++ndx < _Classes.Keys.Count)
                        {
                            classNames += ", ";
                        }
                    }
                    ClassNames = classNames;

                    // set the derived ClassLevels string property
                    var classLevels = "";
                    ndx = 0;
                    foreach (var level in _Classes.Values)
                    {
                        classLevels += $"{level}";
                        if (++ndx < _Classes.Values.Count)
                        {
                            classLevels += ", ";
                        }
                    }
                    ClassLevels = classLevels;
                }
                OnPropertyChanged();
            }
        }
        // is pet name known?
        public bool IsPetNameKnown => _PetName != "";
        public int RankIndex { get; private set; } = -1;
        private int maxObservedMelee = 0;
        public void CheckMaxMelee(int damage)
        {
            if (PetSpell != null && IsPetNameKnown)
            {
                // new high?
                if (damage > maxObservedMelee || RankIndex == -1)
                {
                    maxObservedMelee = damage;
                    // walk the list of ranks and see if this matches a rank
                    //traverse biggest to smallest so match faster
                    for (var ndx = _PetSpell.PetRankList.Count - 1; ndx >= 0; ndx--)
                    {
                        var petRank = _PetSpell.PetRankList[ndx];
                        if (damage >= petRank.MaxMelee)
                        {
                            RankIndex = ndx;
                            HighLightRow(RankIndex);
                            break;
                        }
                    }
                }
            }
        }

        private string _ClassNames = "";
        public string ClassNames
        {
            get => _ClassNames;
            set { _ClassNames = value; OnPropertyChanged(); }
        }

        private string _ClassLevels = "";
        public string ClassLevels
        {
            get => _ClassLevels;
            set { _ClassLevels = value; OnPropertyChanged(); }
        }


        // reagents, kept in a list of pairs of (PetReagent enum, number of that reagent required)
        private List<Tuple<PetReagent, int>> _PetReagents = new List<Tuple<PetReagent, int>>();
        public List<Tuple<PetReagent, int>> PetReagents
        {
            get => _PetReagents;
            set
            {
                // set the field value
                _PetReagents = value;
                PetReagentsText = "";

                if (_PetReagents != null)
                {
                    // set the dervied string property
                    var reagentText = "";
                    var ndx = 0;
                    foreach (var pair in _PetReagents)
                    {
                        reagentText += $"{pair.Item2}x {pair.Item1}";
                        if (++ndx < _PetReagents.Count)
                        {
                            reagentText += ", ";
                        }
                    }
                    PetReagentsText = reagentText;
                }
                OnPropertyChanged();
            }
        }

        private string _PetReagentsText = "";
        public string PetReagentsText
        {
            get => _PetReagentsText;
            set { _PetReagentsText = value; OnPropertyChanged(); }
        }

        // list of PetRank objects
        private List<PetRank> _PetRankList = new List<PetRank>();
        public List<PetRank> PetRankList
        {
            get => _PetRankList;
            set { _PetRankList = value; OnPropertyChanged(); }
        }


        // data to support dynamic highlighting of any given row
        private readonly System.Windows.Media.Brush _HighLightColor = System.Windows.Media.Brushes.DarkGreen;
        private readonly System.Windows.Media.Brush _NormalColor = System.Windows.Media.Brushes.Transparent;

        public List<System.Windows.Media.Brush> RowColor { get; } = new List<System.Windows.Media.Brush>();

        // un-highlight all row colors
        private void ResetRowBackgrounds()
        {
            for (var ndx = 0; ndx < RankRowsCount; ndx++)
            {
                RowColor[ndx] = _NormalColor;
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
                RowColor[ndx] = _HighLightColor;

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
