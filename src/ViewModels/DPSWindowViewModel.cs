using EQTool.Models;
using EQTool.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class DPSWindowViewModel : INotifyPropertyChanged
    {
        private readonly IAppDispatcher appDispatcher;
        private DateTime? LastTimeFighting;

        public DPSWindowViewModel(IAppDispatcher appDispatcher)
        {
            this.appDispatcher = appDispatcher;
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

        public void UpdateDPS()
        {
            appDispatcher.DispatchUI(() =>
            {
                foreach (var item in _EntityList)
                {
                    item.UpdateDps();
                }

                var now = DateTime.Now;
                if (LastTimeFighting.HasValue && (now - LastTimeFighting.Value).TotalSeconds > 20)
                {
                    LastTimeFighting = null;
                    var itemstoremove = EntityList.ToList();
                    foreach (var item in itemstoremove)
                    {
                        _ = EntityList.Remove(item);
                    }
                }
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
                var deadguys = EntityList.Where(a => a.TargetName == target).ToList();
                foreach (var item in deadguys)
                {
                    item.IsDead = true;
                }
            });
        }

        public void TryAdd(DPSParseMatch entitiy)
        {
            if (entitiy == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                LastTimeFighting = DateTime.Now;
                var item = EntityList.FirstOrDefault(a => a.SourceName == entitiy.SourceName && a.TargetName == entitiy.TargetName);
                if (item == null)
                {
                    EntityList.Add(new EntittyDPS
                    {
                        SourceName = entitiy.SourceName,
                        TargetName = entitiy.TargetName,
                        StartTime = DateTime.Now,
                        TotalDamage = entitiy.DamageDone
                    });
                }
                else
                {
                    item.AddDamage(new EntittyDPS.DamagePerTime
                    {
                        TimeStamp = entitiy.TimeStamp,
                        Damage = entitiy.DamageDone
                    });
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
