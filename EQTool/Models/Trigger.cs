using EQToolShared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EQTool.Models
{
    [Serializable]
    public class ServerTrigger
    {
        public Servers Servers { get; set; }
        public Trigger Trigger { get; set; }
    }

    [Serializable]
    public class Trigger : INotifyPropertyChanged
    {
        private const string ginaRegexPattern = @"\{(?<xxx>\w+)\}";
        private static Regex ginaRegex = new Regex(ginaRegexPattern, RegexOptions.Compiled);

        private bool _TriggerEnabled;
        public bool TriggerEnabled
        {
            get { return this._TriggerEnabled; }
            set
            {
                if (this._TriggerEnabled != value)
                {
                    this._TriggerEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _TriggerName = string.Empty;
        public string TriggerName
        {
            get { return this._TriggerName; }
            set
            {
                if (this._TriggerName != value)
                {
                    this._TriggerName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _TriggerRegExString = string.Empty;
        public string TriggerRegExString
        {
            get { return this._TriggerRegExString; }
            set
            {
                if (this._TriggerRegExString != value)
                {
                    this._TriggerRegExString = value;
                    this.OnPropertyChanged(nameof(TriggerRegex));
                    this.OnPropertyChanged();
                }
            }
        }
        private List<(string Name, string Value)> SimpleMapping = new List<(string Name, string Value)>();
        private string _TriggerRegExStringConverted = string.Empty;

        private Regex _TriggerRegex;
        private Regex TriggerRegex
        {
            get
            {
                //delay regex creation!
                if (this._TriggerRegex == null && !string.IsNullOrWhiteSpace(this._TriggerRegExString))
                {
                    try
                    {
                        this._TriggerRegExStringConverted = this._TriggerRegExString;
                        var match = ginaRegex.Match(_TriggerRegExStringConverted);
                        while (match.Success)
                        {
                            string group_name = match.Groups["xxx"].Value;
                            _TriggerRegExStringConverted = ginaRegex.Replace(_TriggerRegExStringConverted, $"(?<{group_name}>[\\w` ]+)", 1);
                            SimpleMapping.Add((group_name, string.Empty));
                            match = match.NextMatch();
                        }
                        this._TriggerRegex = new Regex(_TriggerRegExString, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    }
                    catch
                    {
                        this._TriggerRegex = null;
                        this._TriggerRegExStringConverted = string.Empty;
                    }
                }
                return this._TriggerRegex;
            }
        }

        public static bool Match(Trigger userTrigger, string line)
        {
            if (userTrigger._TriggerRegex == null)
            {
                return false;
            }

            var match = userTrigger._TriggerRegex.Match(line);
            if (match.Success)
            {
                foreach (Group g in match.Groups)
                {
                    if (!userTrigger.SimpleMapping.Any(a => string.Equals(a.Name, g.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        userTrigger.SimpleMapping.Add((g.Name, g.Value));
                    }
                }
            }
            return match.Success;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}