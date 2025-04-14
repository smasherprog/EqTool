using EQTool.Models;
using EQTool.Services;
using EQToolShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace EQTool.ViewModels
{
    public class DPSWindowViewModel : INotifyPropertyChanged
    {
        private readonly IAppDispatcher appDispatcher;
        public DPSWindowViewModel(IAppDispatcher appDispatcher, ActivePlayer activePlayer, SessionPlayerDamage sessionPlayerDamage)
        {
            this.appDispatcher = appDispatcher;
            Title = "Dps Meter v" + App.Version;
            ActivePlayer = activePlayer;
            SessionPlayerDamage = sessionPlayerDamage;
        }

        public ObservableCollection<EntittyDPS> _EntityList = new ObservableCollection<EntittyDPS>();
        public ObservableCollection<EntittyDPS> EntityList
        {
            get => _EntityList;
            set
            {
                _EntityList = value;
                OnPropertyChanged();
            }
        }

        public void UpdateStuff()
        {
            OnPropertyChanged(nameof(EntityList));
        }

        private SessionPlayerDamage _LastPlayerDamage = null;
        public SessionPlayerDamage LastPlayerDamage
        {
            get => _LastPlayerDamage;
            set
            {
                _LastPlayerDamage = value;
                OnPropertyChanged();
            }
        }

        private SessionPlayerDamage _SessionPlayerDamage = null;
        public SessionPlayerDamage SessionPlayerDamage
        {
            get => _SessionPlayerDamage;
            set
            {
                _SessionPlayerDamage = value;
                OnPropertyChanged();
            }
        }

        private ActivePlayer _activePlayer = null;
        public ActivePlayer ActivePlayer
        {
            get => _activePlayer;
            set
            {
                _activePlayer = value;
                OnPropertyChanged();
            }
        }

        private string _Title = null;
        public string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }

        public static bool ShouldRemove(DateTime now, DateTime? lastdmgdone, DateTime startime, int groupcount)
        {
            var lasttime = lastdmgdone.HasValue && lastdmgdone.Value > startime ? lastdmgdone.Value : startime;
            var timeup = Math.Abs((now - lasttime).TotalSeconds);
            var secondstosubtract = groupcount * 20;
            secondstosubtract = Math.Min(60, secondstosubtract);
            _ = 80 - secondstosubtract;
            return timeup > 40;
        }

        public void UpdateDPS()
        {
            appDispatcher.DispatchUI(() =>
            {
                var itemstormove = new List<EntittyDPS>();
                var now = DateTime.Now;
                var groups = _EntityList.GroupBy(a => a.TargetName).ToList();
                foreach (var item in _EntityList)
                {
                    if (ShouldRemove(now, item.LastDamageDone, item.StartTime, groups.Count))
                    {
                        itemstormove.Add(item);
                    }
                    else
                    {
                        item.UpdateDps();
                    }
                }

                foreach (var item in groups)
                {
                    var totaldmg = item.Sum(a => a.TotalDamage);
                    foreach (var e in item)
                    {
                        e.TargetTotalDamage = totaldmg;
                    }
                }
                foreach (var item in itemstormove)
                {
                    _ = EntityList.Remove(item);
                }

                var you = _EntityList.FirstOrDefault(a => a.SourceName == "You" && a.TotalSeconds > 20);
                if (you != null)
                {
                    if (ActivePlayer.Player != null)
                    {
                        ActivePlayer.Player.BestPlayerDamage.HighestDPS = Math.Max(ActivePlayer.Player.BestPlayerDamage.HighestDPS, you.DPS);
                        ActivePlayer.Player.BestPlayerDamage.TargetTotalDamage = Math.Max(ActivePlayer.Player.BestPlayerDamage.TargetTotalDamage, you.TotalDamage);
                        ActivePlayer.Player.BestPlayerDamage.HighestHit = Math.Max(ActivePlayer.Player.BestPlayerDamage.HighestHit, you.HighestHit);
                    }
                    //this.OnPropertyChanged(nameof(ActivePlayer));
                    SessionPlayerDamage.CurrentSessionPlayerDamage.HighestDPS = Math.Max(SessionPlayerDamage.CurrentSessionPlayerDamage.HighestDPS, you.DPS);
                    SessionPlayerDamage.CurrentSessionPlayerDamage.TargetTotalDamage = Math.Max(SessionPlayerDamage.CurrentSessionPlayerDamage.TargetTotalDamage, you.TotalDamage);
                    SessionPlayerDamage.CurrentSessionPlayerDamage.HighestHit = Math.Max(SessionPlayerDamage.CurrentSessionPlayerDamage.HighestHit, you.HighestHit);
                    //this.OnPropertyChanged(nameof(SessionPlayerDamage));
                }
                var view = (ListCollectionView)CollectionViewSource.GetDefaultView(EntityList);
                view.Refresh();
            });
        }

        public void TargetDied(string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var t = target.ToLower();
                var itemstoremove = EntityList.Where(a => a.TargetName.ToLower() == t).ToList();
                foreach (var item in itemstoremove)
                {
                    item.DeathTime = DateTime.Now;
                }
            });
        }

        public void TryAdd(DamageEvent entity)
        {
            //when charmed pet and nps have the same name, everything is messed up
            if (entity == null || entity.AttackerName == entity.TargetName)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                var item = EntityList.FirstOrDefault(a => a.SourceName == entity.AttackerName && a.TargetName == entity.TargetName && !a.DeathTime.HasValue);
                if (item == null)
                {
                    var istargetnpc = MasterNPCList.NPCs.Contains(entity.TargetName);
                    var issourcenpc = MasterNPCList.NPCs.Contains(entity.AttackerName);
                    item = new EntittyDPS
                    {
                        SourceName = entity.AttackerName,
                        TargetName = entity.TargetName,
                        StartTime = entity.TimeStamp,
                        TotalDamage = entity.DamageDone,
                        TotalTwelveSecondDamage = entity.DamageDone,
                        TrailingDamage = entity.DamageDone,
                        HighestHit = entity.DamageDone,
                        Level = entity.LevelGuess,
                        isSourceNpc = issourcenpc,
                        isTargetNpc = istargetnpc,
                    };
                    EntityList.Add(item);
                }
                else
                {
                    item.Level = entity.LevelGuess;
                    //Debug.WriteLine($"{entity.TargetName} {entity.DamageDone}");
                    item.AddDamage(new EntittyDPS.DamagePerTime
                    {
                        TimeStamp = entity.TimeStamp,
                        Damage = entity.DamageDone
                    });
                }
                foreach (var it in EntityList.Where(a => a.TargetName == entity.AttackerName))
                {
                    it.Level = entity.LevelGuess;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
