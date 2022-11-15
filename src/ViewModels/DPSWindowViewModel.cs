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
        private readonly EQToolSettings settings;
        private DateTime? LastTimeFighting;

        public DPSWindowViewModel(IAppDispatcher appDispatcher, EQToolSettings settings)
        {
            this.appDispatcher = appDispatcher;
            this.settings = settings;
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

        public void TryAdd(EntittyDPS entitiy)
        {
            if (entitiy == null)
            {
                return;
            }

            appDispatcher.DispatchUI(() =>
            {
                LastTimeFighting = DateTime.Now;
                var item = EntityList.FirstOrDefault(a => a.Name == entitiy.Name);
                if (item == null)
                {
                    EntityList.Add(entitiy);
                }
                else
                {
                    item.TotalDamage += entitiy.TotalDamage;
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
