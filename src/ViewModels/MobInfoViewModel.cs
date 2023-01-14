using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace EQTool.ViewModels
{
    public class MobInfoViewModel : INotifyPropertyChanged
    {
        public string Title = "Mob Info";
        private string _Results = string.Empty;

        public string Results
        {
            get => _Results;
            set
            {
                _Results = value;
                Parse();
                OnPropertyChanged();
            }
        }

        private string _Url = string.Empty;

        public string Url
        {
            get => _Url;
            set
            {
                _Url = value;
                OnPropertyChanged();
            }
        }

        private string _Name = string.Empty;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private string _Race = string.Empty;

        public string Race
        {
            get => _Race;
            set
            {
                _Race = value;
                OnPropertyChanged();
            }
        }

        private string _Class = string.Empty;

        public string Class
        {
            get => _Class;
            set
            {
                _Class = value;
                OnPropertyChanged();
            }
        }

        private string _Level = string.Empty;

        public string Level
        {
            get => _Level;
            set
            {
                _Level = value;
                OnPropertyChanged();
            }
        }

        private string _AgroRadius = string.Empty;

        public string AgroRadius
        {
            get => _AgroRadius;
            set
            {
                _AgroRadius = value;
                OnPropertyChanged();
            }
        }

        private string _RunSpeed = string.Empty;

        public string RunSpeed
        {
            get => _RunSpeed;
            set
            {
                _RunSpeed = value;
                OnPropertyChanged();
            }
        }
        private string _AC = string.Empty;

        public string AC
        {
            get => _AC;
            set
            {
                _AC = value;
                OnPropertyChanged();
            }
        }
        private string _HP = string.Empty;

        public string HP
        {
            get => _HP;
            set
            {
                _HP = value;
                OnPropertyChanged();
            }
        }
        private string _HPRegen = string.Empty;

        public string HPRegen
        {
            get => _HPRegen;
            set
            {
                _HPRegen = value;
                OnPropertyChanged();
            }
        }

        private string _ManaRegen = string.Empty;

        public string ManaRegen
        {
            get => _ManaRegen;
            set
            {
                _ManaRegen = value;
                OnPropertyChanged();
            }
        }
        private string _AttacksPerRound = string.Empty;

        public string AttacksPerRound
        {
            get => _AttacksPerRound;
            set
            {
                _AttacksPerRound = value;
                OnPropertyChanged();
            }
        }
        private string _AttackSpeed = string.Empty;

        public string AttackSpeed
        {
            get => _AttackSpeed;
            set
            {
                _AttackSpeed = value;
                OnPropertyChanged();
            }
        }
        private string _DamagePerHit = string.Empty;

        public string DamagePerHit
        {
            get => _DamagePerHit;
            set
            {
                _DamagePerHit = value;
                OnPropertyChanged();
            }
        }
        private string _Special = string.Empty;

        public string Special
        {
            get => _Special;
            set
            {
                _Special = value;
                OnPropertyChanged();
            }
        }

        private void Parse()
        {
            if (string.IsNullOrWhiteSpace(Results))
            {
                return;
            }
            var cleanresults = Results.Replace("\r\n", "\n");
            var splits = cleanresults.Split('\n').Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            Name = GetValue("name", splits);
            Race = GetValue("race", splits);
            Class = GetValue("class", splits)?.Replace("[[", string.Empty).Replace("]]", string.Empty);
            Level = GetValue("level", splits);
            AgroRadius = GetValue("agro_radius", splits);
            RunSpeed = GetValue("run_speed", splits);
            AC = GetValue("AC", splits);
            HP = GetValue("HP", splits);
            HPRegen = GetValue("HP_regen", splits);
            ManaRegen = GetValue("mana_regen", splits);
            AttacksPerRound = GetValue("attacks_per_round", splits);
            AttackSpeed = GetValue("attack_speed", splits);
            DamagePerHit = GetValue("damage_per_hit", splits);
            Special = GetValue("special", splits);

            if (!string.IsNullOrWhiteSpace(Name))
            {
                var name = HttpUtility.UrlEncode(Name.Replace(' ', '_'));
                Url = $"https://wiki.project1999.com/{name}";
            }
        }

        private string GetValue(string propname, List<string> lines)
        {
            return lines.FirstOrDefault(a => a.StartsWith("| " + propname))?.Split('=')?.LastOrDefault()?.Trim();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
