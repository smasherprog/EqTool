using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels.MobInfoComponents
{
    public class PetViewModel : INotifyPropertyChanged
    {
        // 5 normal ranks plus 1 focused rank
        private const int RankRowsCount = 6;

        public PetViewModel()
        {
            for (var ndx = 0; ndx < RankRowsCount; ndx++)
            {
                RowColor.Add(new System.Windows.Media.SolidColorBrush());
            }
            ResetRowBackgrounds();
        }

        private string _PetName = "";
        public string PetName
        {
            get => _PetName;
            set { _PetName = value; OnPropertyChanged(); }
        }

        private PetSpell _PetSpell = null;
        public PetSpell PetSpell
        {
            get => _PetSpell;
            set
            {
                _PetSpell = value;

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

        private string _SpellName = "";
        public string SpellName
        {
            get => _SpellName;
            set { _SpellName = value; OnPropertyChanged(); }
        }

        private Dictionary<PlayerClasses, int> _Classes = new Dictionary<PlayerClasses, int>();
        public Dictionary<PlayerClasses, int> Classes
        {
            get => _Classes;
            set
            {
                _Classes = value;
                ClassNames = "";
                ClassLevels = "";

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
        public bool IsPetNameKnown => _PetName != "";
        public int RankIndex { get; private set; } = -1;
        private int maxObservedMelee = 0;
        public void CheckMaxMelee(int damage)
        {
            if (PetSpell != null && IsPetNameKnown)
            {
                if (damage > maxObservedMelee || RankIndex == -1)
                {
                    maxObservedMelee = damage;
                    // biggest to smallest so the common high ranks match first
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


        private List<Tuple<PetReagent, int>> _PetReagents = new List<Tuple<PetReagent, int>>();
        public List<Tuple<PetReagent, int>> PetReagents
        {
            get => _PetReagents;
            set
            {
                _PetReagents = value;
                PetReagentsText = "";

                if (_PetReagents != null)
                {
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

        private List<PetRank> _PetRankList = new List<PetRank>();
        public List<PetRank> PetRankList
        {
            get => _PetRankList;
            set { _PetRankList = value; OnPropertyChanged(); }
        }


        // must stay visible behind both black and white font
        private readonly System.Windows.Media.Brush _HighLightColor = System.Windows.Media.Brushes.LightSlateGray;
        private readonly System.Windows.Media.Brush _NormalColor = System.Windows.Media.Brushes.Transparent;
        //private readonly System.Windows.Media.Brush _HighLightColor = System.Windows.Media.Brushes.DarkGreen;
        //private readonly System.Windows.Media.Brush _HighLightColor = System.Windows.Media.Brushes.LightGreen;

        public List<System.Windows.Media.Brush> RowColor { get; } = new List<System.Windows.Media.Brush>();

        private void ResetRowBackgrounds()
        {
            for (var ndx = 0; ndx < RankRowsCount; ndx++)
            {
                RowColor[ndx] = _NormalColor;
                //RowColor[ndx] = _HighLightColor;

                OnPropertyChanged(nameof(RowColor));
            }
        }

        public void HighLightRow(int ndx)
        {
            if ((ndx >= 0) && (ndx < RankRowsCount))
            {
                ResetRowBackgrounds();
                RowColor[ndx] = _HighLightColor;

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
