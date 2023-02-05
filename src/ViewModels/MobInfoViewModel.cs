using EQTool.Services.Parsing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;

namespace EQTool.ViewModels
{
    public class TestUriViewModel : INotifyPropertyChanged
    {
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

        private string _Url = string.Empty;

        public string Url
        {
            get => _Url;
            set
            {
                _Url = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HaseUrl));
                OnPropertyChanged(nameof(HaseNoUrl));
            }
        }

        public Visibility HaseNoUrl => string.IsNullOrWhiteSpace(Url) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility HaseUrl => string.IsNullOrWhiteSpace(Url) ? Visibility.Collapsed : Visibility.Visible;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class MobInfoViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = "Mob Info";
        private string _Results = string.Empty;

        public string Results
        {
            get => _Results;
            set
            {
                _Results = value;
                _ErrorResults = string.Empty;
                Parse();
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrors));
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        private string _ErrorResults = string.Empty;

        public string ErrorResults
        {
            get => _ErrorResults;
            set
            {
                _ErrorResults = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasErrors));
                OnPropertyChanged(nameof(HasNoErrors));
            }
        }

        public Visibility HasNoErrors => !string.IsNullOrWhiteSpace(_ErrorResults) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility HasErrors => !string.IsNullOrWhiteSpace(_ErrorResults) ? Visibility.Visible : Visibility.Collapsed;

        private string _Url = string.Empty;

        public string Url
        {
            get => _Url;
            set
            {
                _Url = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HaseUrl));
            }
        }

        public Visibility HaseUrl => string.IsNullOrWhiteSpace(Url) ? Visibility.Collapsed : Visibility.Visible;

        private string _ImageUrl = string.Empty;

        public string ImageUrl
        {
            get => _ImageUrl;
            set
            {
                _ImageUrl = value;
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

        private ObservableCollection<TestUriViewModel> _Specials = new ObservableCollection<TestUriViewModel>();

        public ObservableCollection<TestUriViewModel> Specials
        {
            get => _Specials;
            set
            {
                _Specials = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TestUriViewModel> _KnownLoot = new ObservableCollection<TestUriViewModel>();

        public ObservableCollection<TestUriViewModel> KnownLoot
        {
            get => _KnownLoot;
            set
            {
                _KnownLoot = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TestUriViewModel> _Factions = new ObservableCollection<TestUriViewModel>();

        public ObservableCollection<TestUriViewModel> Factions
        {
            get => _Factions;
            set
            {
                _Factions = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TestUriViewModel> _OpposingFactions = new ObservableCollection<TestUriViewModel>();

        public ObservableCollection<TestUriViewModel> OpposingFactions
        {
            get => _OpposingFactions;
            set
            {
                _OpposingFactions = value;
                OnPropertyChanged();
            }
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        private void Parse()
        {
            if (string.IsNullOrWhiteSpace(Results))
            {
                return;
            }
            var spec = Specials.ToList();
            foreach (var item in spec)
            {
                _ = Specials.Remove(item);
            }
            spec = KnownLoot.ToList();
            foreach (var item in spec)
            {
                _ = KnownLoot.Remove(item);
            }
            spec = Factions.ToList();
            foreach (var item in spec)
            {
                _ = Factions.Remove(item);
            }
            spec = OpposingFactions.ToList();
            foreach (var item in spec)
            {
                _ = OpposingFactions.Remove(item);
            }
            var cleanresults = Results.Replace("\r\n", "\n").Replace("|imagefilename", "^imagefilename").Replace("| ", "^");
            var splits = cleanresults.Split('^').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim().TrimStart('\n')).ToList();
            Name = GetValue("name", splits);
            Race = GetValue("race", splits);
            Class = GetValue("class", splits)?.Replace("[[", string.Empty).Replace("]]", string.Empty);
            var lvl = GetValue("level", splits)?.Where(a => char.IsDigit(a) || a == ' ' || a == '-')?.ToArray();
            if (lvl != null)
            {
                Level = new string(lvl);
            }

            AgroRadius = GetValue("agro_radius", splits);
            RunSpeed = GetValue("run_speed", splits);
            AC = GetValue("AC", splits);
            HP = GetValue("HP", splits);
            HPRegen = GetValue("HP_regen", splits);
            ManaRegen = GetValue("mana_regen", splits);
            AttacksPerRound = GetValue("attacks_per_round", splits);
            AttackSpeed = GetValue("attack_speed", splits);
            DamagePerHit = GetValue("damage_per_hit", splits);
            var specials = StripHTML(GetValue("special", splits)).Split(new[] { ',', '\n' });
            foreach (var item in specials)
            {
                if (item.Contains("[["))
                {
                    var name = item.Replace("[[", string.Empty).Replace("]]", string.Empty).Trim('*').Trim();
                    Specials.Add(new TestUriViewModel
                    {
                        Url = $"https://wiki.project1999.com/" + name.Replace(' ', '_'),
                        Name = name
                    });
                }
                else
                {
                    Specials.Add(new TestUriViewModel
                    {
                        Url = string.Empty,
                        Name = item.Trim()
                    });
                }
            }
            var known_loot = MobInfoParsing.ParseKnwonLoot(splits);
            foreach (var item in known_loot)
            {
                KnownLoot.Add(item);
            }

            specials = StripHTML(GetValue("factions", splits)).Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in specials)
            {
                var indexof = item.IndexOf("[[");
                if (indexof != -1)
                {
                    var indexofend = item.IndexOf("]]");
                    if (indexofend != -1)
                    {
                        var model = new TestUriViewModel();
                        var diff = indexofend - (indexof + 2);
                        model.Name = item.Substring(indexof + 2, diff).Trim();
                        model.Url = $"https://wiki.project1999.com/" + model.Name.Replace(' ', '_');
                        model.Name += "  " + item.Substring(indexofend + 2).Trim();
                        Factions.Add(model);
                    }
                }
            }

            specials = StripHTML(GetValue("opposing_factions", splits)).Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in specials)
            {
                var indexof = item.IndexOf("[[");
                if (indexof != -1)
                {
                    var indexofend = item.IndexOf("]]");
                    if (indexofend != -1)
                    {
                        var model = new TestUriViewModel();
                        var diff = indexofend - (indexof + 2);
                        model.Name = item.Substring(indexof + 2, diff).Trim();
                        model.Url = $"https://wiki.project1999.com/" + model.Name.Replace(' ', '_');
                        model.Name += "  " + item.Substring(indexofend + 2).Trim();
                        OpposingFactions.Add(model);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                var name = HttpUtility.UrlEncode(Name.Replace(' ', '_'));
                Url = $"https://wiki.project1999.com/{name}";
            }

            var imageurl = GetValue("imagefilename", splits);
            if (!string.IsNullOrWhiteSpace(imageurl) && imageurl.Length > 2)
            {
                imageurl = char.ToUpper(imageurl[0]) + imageurl.Substring(1, imageurl.Length - 1);
                ImageUrl = $"https://wiki.project1999.com/images/{imageurl}";
            }
        }

        private string GetValue(string propname, List<string> lines)
        {
            var ret = lines.FirstOrDefault(a => a.StartsWith(propname));
            if (string.IsNullOrWhiteSpace(ret))
            {
                return string.Empty;
            }
            var index = ret.IndexOf('=');
            return index != -1 ? ret.Substring(index + 1).Trim() : string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
