using EQTool.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class ZoneViewModel : INotifyPropertyChanged
    {
        private readonly WikiApi wikiApi;

        public ZoneViewModel(WikiApi wikiApi)
        {
            this.wikiApi = wikiApi;
        }

        private string _Name = string.Empty;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                if (!string.IsNullOrWhiteSpace(_Name))
                {
                    var cleanname = new List<string>();
                    foreach (var item in _Name.Split(' '))
                    {
                        var itemclean = item.Trim();
                        if (itemclean == "of")
                        {
                            cleanname.Add(itemclean);
                        }
                        else
                        {
                            cleanname.Add(char.ToUpper(itemclean[0]) + itemclean.Substring(1));
                        }
                    }
                    _CleanedName = string.Join(" ", cleanname);
                }
                OnPropertyChanged();
            }
        }

        private string _CleanedName = string.Empty;

        public string CleanedName
        {
            get => _CleanedName;
            set
            {
                _CleanedName = value;
                OnPropertyChanged();
            }
        }

        private string _RespawnTime = string.Empty;

        public string RespawnTime
        {
            get => _RespawnTime;
            set
            {
                _RespawnTime = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
