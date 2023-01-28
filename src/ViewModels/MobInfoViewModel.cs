using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace EQTool.ViewModels
{
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
        private ObservableCollection<TextBlock> _Specials = new ObservableCollection<TextBlock>();

        public ObservableCollection<TextBlock> Specials
        {
            get => _Specials;
            set
            {
                _Specials = value;
                OnPropertyChanged();
            }
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _ = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigatebutton(object sender, RoutedEventArgs args)
        {
            var s = sender as Button;
            var h = (s.Parent as System.Windows.Documents.InlineUIContainer).Parent as Hyperlink;
            _ = Process.Start(new ProcessStartInfo(h.NavigateUri.AbsoluteUri));
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

            var cleanresults = Results.Replace("\r\n", "\n");
            var splits = cleanresults.Split('\n').Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
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
            var specials = GetValue("special", splits).Replace("<br />", string.Empty).Replace("<br/>", string.Empty).Replace("<br>", string.Empty).Replace("<br >", string.Empty).Split(',');
            foreach (var item in specials)
            {
                if (item.Contains("[["))
                {
                    var wrappertext = new TextBlock();
                    var hyperlink = new Hyperlink();
                    var name = item.Replace("[[", string.Empty).Replace("]]", string.Empty).Trim();
                    hyperlink.NavigateUri = new System.Uri($"https://wiki.project1999.com/{name}");
                    hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                    hyperlink.Inlines.Add(new TextBlock { Text = name, Padding = new Thickness(2, 0, 2, 0) });
                    var b = new Button { Margin = new Thickness(1), FontSize = 7, Width = 14, Height = 14, ToolTip = "Open in web Browser", Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/HyperlinkForward.png")) } };
                    b.Click += Hyperlink_RequestNavigatebutton;
                    hyperlink.Inlines.Add(b);
                    wrappertext.Inlines.Add(hyperlink);
                    Specials.Add(wrappertext);
                }
                else
                {
                    var wrappertext = new TextBlock { Text = item };
                    Specials.Add(wrappertext);
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
            return lines.FirstOrDefault(a => a.StartsWith("| " + propname))?.Split('=')?.LastOrDefault()?.Trim();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
