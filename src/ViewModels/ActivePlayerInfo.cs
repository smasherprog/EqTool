using EQTool.Models;
using EQToolShared.Enums;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class ActivePlayer : INotifyPropertyChanged
    {
        private readonly EQToolSettings settings;
        public ActivePlayer(EQToolSettings settings)
        {
            this.settings = settings;
        }

        public static PlayerInfo GetInfoFromString(string logfilenbame)
        {
            var charname_withext = logfilenbame.Replace("eqlog_", string.Empty);
            var indexpart = charname_withext.IndexOf("_");
            var charName = charname_withext.Substring(0, indexpart);

            var p = new PlayerInfo
            {
                Level = 1,
                Name = charName,
                PlayerClass = null,
                Zone = "freportw",
                ShowSpellsForClasses = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().ToList()
            };

            indexpart = charname_withext.LastIndexOf("_");
            if (indexpart != -1)
            {
                var server = charname_withext.Substring(indexpart + 1).Replace(".txt", string.Empty);
                if (server == "P1999PVP")
                {
                    p.Server = Servers.Red;
                }
                else if (server == "P1999Green")
                {
                    p.Server = Servers.Green;
                }
                else
                {
                    p.Server = Servers.Blue;
                }
            }

            if (logfilenbame.IndexOf("pq.proj.txt") != -1)
            {
                p.Server = Servers.Quarm;
            }

            return p;
        }
        public bool Update()
        {
            var playerchanged = false;
            try
            {
                var players = settings.Players ?? new System.Collections.Generic.List<PlayerInfo>();
                var directory = new DirectoryInfo(settings.EqLogDirectory);
                var loggedincharlogfile = directory.GetFiles("eqlog*.txt", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(a => a.LastWriteTime)
                    .FirstOrDefault();

                if (loggedincharlogfile != null)
                {
                    var parseinfo = GetInfoFromString(loggedincharlogfile.Name);
                    var tempplayer = players.FirstOrDefault(a => a.Name == parseinfo.Name);
                    LogFileName = loggedincharlogfile.FullName;

                    if (tempplayer == null)
                    {
                        players.Add(parseinfo);
                    }
                    else
                    {
                        tempplayer.Server = parseinfo.Server;
                    }

                    playerchanged = tempplayer != Player;
                    Player = tempplayer;
                }
                else
                {
                    Player = null;
                }
            }
            catch
            {

            }

            return playerchanged;
        }

        private Spell _UserCastingSpell;

        public Spell UserCastingSpell
        {
            get => _UserCastingSpell;
            set
            {
                _UserCastingSpell = value;
                OnPropertyChanged();
            }
        }

        public int UserCastingSpellCounter { get; set; }

        public string LogFileName;

        private PlayerInfo _Player;

        public PlayerInfo Player
        {
            get => _Player;
            set
            {
                _Player = value;
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
