using EQTool.Models;
using EQTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class SpellWindowViewModel : INotifyPropertyChanged
    {
        private readonly ActivePlayer activePlayer;
        private readonly IAppDispatcher appDispatcher;

        public SpellWindowViewModel(ActivePlayer activePlayer, IAppDispatcher appDispatcher)
        {
            this.activePlayer = activePlayer;
            this.appDispatcher = appDispatcher;
        }

        public ObservableCollection<UISpell> _SpellList = new ObservableCollection<UISpell>();
        public ObservableCollection<UISpell> SpellList
        {
            get => _SpellList;
            set
            {
                _SpellList = value;
                OnPropertyChanged();
            }
        }

        private long? _LastReadOffset = null;
        public long? LastReadOffset
        {
            get => _LastReadOffset;
            set
            {
                _LastReadOffset = value;
                OnPropertyChanged();
            }
        }

        public void UpdateSpells()
        {
            appDispatcher.DispatchUI(() =>
            {
                var itemstoremove = new List<UISpell>();
                foreach (var item in SpellList)
                {
                    item.SecondsLeftOnSpell = TimeSpan.FromSeconds(item.SecondsLeftOnSpell.TotalSeconds - 1);
                    if (item.SecondsLeftOnSpell.TotalSeconds <= 0)
                    {
                        itemstoremove.Add(item);
                    }
                }

                foreach (var item in itemstoremove)
                {
                    _ = SpellList.Remove(item);
                }
            });
        }

        public void TryAdd(Spell spell, string target)
        {
            if (spell == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                _ = activePlayer.Update();
                var s = SpellList.FirstOrDefault(a => a.SpellName == spell.name && a.TargetName == target);
                if (s != null)
                {
                    _ = SpellList.Remove(s);
                }

                var level = activePlayer.Player?.Level;
                var spellduration = TimeSpan.FromSeconds(SpellDurations.GetDuration_inSeconds(spell, level));
                SpellList.Add(new UISpell
                {
                    TotalSecondsOnSpell = (int)spellduration.TotalSeconds,
                    PercentLeftOnSpell = 100,
                    SpellType = spell.type,
                    TargetName = target,
                    SpellName = spell.name,
                    Rect = spell.Rect,
                    SecondsLeftOnSpell = spellduration,
                    SpellIcon = spell.SpellIcon
                });
            });
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
