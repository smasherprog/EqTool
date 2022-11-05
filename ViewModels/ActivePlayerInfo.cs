using EQTool.Models;
using EQTool.Services;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels
{
    public class ActivePlayer : INotifyPropertyChanged
    {
        private readonly EQToolSettings settings;
        private readonly IAppDispatcher appDispatcher;
        public ActivePlayer(EQToolSettings settings, IAppDispatcher appDispatcher)
        {
            this.settings = settings;
            this.appDispatcher = appDispatcher;
        }

        public bool Update()
        {
            var players = settings.Players ?? new System.Collections.Generic.List<PlayerInfo>();
            var directory = new DirectoryInfo(settings.DefaultEqDirectory + "/Logs/");
            var loggedincharlogfile = directory.GetFiles()
                .Where(a => a.Name.StartsWith("eqlog") && a.Name.EndsWith(".txt"))
                .OrderByDescending(a => a.LastWriteTime)
                .FirstOrDefault();
            var playerchanged = false;
            if (loggedincharlogfile != null)
            {
                var charname = loggedincharlogfile.Name.Replace("eqlog_", string.Empty);
                var indexpart = charname.IndexOf("_");
                var charName = charname.Substring(0, indexpart);
                var tempplayer = players.FirstOrDefault(a => a.Name == charName);
                appDispatcher.DispatchUI(() =>
                {
                    LogFileName = loggedincharlogfile.FullName;
                    playerchanged = tempplayer != Player;
                    Player = tempplayer;
                });
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

        private int _UserCastingSpellCounter;

        public int UserCastingSpellCounter
        {
            get => _UserCastingSpellCounter;
            set
            {
                _UserCastingSpellCounter = value;
                OnPropertyChanged();
            }
        }

        private string _LogFileName;

        public string LogFileName
        {
            get => _LogFileName;
            set
            {
                _LogFileName = value;
                OnPropertyChanged();
            }
        }

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
