using EQTool.Models;
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
                    var charname_withext = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                    var indexpart = charname_withext.IndexOf("_");
                    var charName = charname_withext.Substring(0, indexpart);
                    var tempplayer = players.FirstOrDefault(a => a.Name == charName);
                    LogFileName = loggedincharlogfile.FullName;

                    if (tempplayer == null)
                    {
                        tempplayer = new PlayerInfo
                        {
                            Level = 1,
                            Name = charName,
                            PlayerClass = null,
                            Zone = "freportw"
                        };
                        players.Add(tempplayer);
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
