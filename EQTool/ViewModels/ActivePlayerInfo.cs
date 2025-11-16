using EQTool.Models;
using EQTool.Services;
using EQToolShared.Enums;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace EQTool.ViewModels
{
    public class ActivePlayer : INotifyPropertyChanged
    {
        private CancellationTokenSource aoeDebounceTs;
        private readonly EQToolSettings settings;
        private readonly LogEvents logEvents;
        private readonly IAppDispatcher appDispatcher;

        public ActivePlayer(EQToolSettings settings, LogEvents logEvents, IAppDispatcher appDispatcher)
        {
            this.settings = settings;
            this.logEvents = logEvents;
            this.appDispatcher = appDispatcher;
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
                ChChainOverlay = true,
                ChChainWarningAudio = true,
                ChChainWarningOverlay = true,
                ShowSpellsForClasses = Enum.GetValues(typeof(PlayerClasses)).Cast<PlayerClasses>().ToList()
            };

            indexpart = charname_withext.LastIndexOf("_");
            if (indexpart != -1)
            {
                var server = charname_withext.Substring(indexpart + 1).Replace(".txt", string.Empty);
                p.Server = server == "P1999PVP" ? Servers.Red : server == "P1999Green" ? Servers.Green : Servers.Blue;
            }

            return p;
        }

        public bool Update()
        {
            var playerchanged = false;
            try
            {
                var players = settings.Players ?? new System.Collections.Generic.List<PlayerInfo>();
                if (!Directory.Exists(settings.EqLogDirectory))
                {
                    return playerchanged;
                }
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
                    if (Player != null && tempplayer != Player)
                    {
                        logEvents.Handle(new BeforePlayerChangedEvent { TimeStamp = DateTime.Now });
                    }
                    playerchanged = tempplayer != Player;
                    if (tempplayer == null)
                    {
                        tempplayer = parseinfo;
                    }
                    Player = tempplayer;
                    if (Player != null)
                    {
                        Player.LastUpdate = DateTime.Now;
                    }
                }
                else
                {
                    Player = null;
                }
            }
            catch
            {

            }
            if (playerchanged && Player != null)
            {
                logEvents.Handle(new AfterPlayerChangedEvent { TimeStamp = DateTime.Now });

            }
            return playerchanged;
        }

        private Spell _UserCastingSpell;
        public Spell UserCastingSpell
        {
            get => _UserCastingSpell;
            private set
            {
                _UserCastingSpell = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _UserCastSpellDateTime;
        public DateTime? UserCastSpellDateTime
        {
            get => _UserCastSpellDateTime;
            set
            {
                _UserCastSpellDateTime = value;
                OnPropertyChanged();
            }
        }

        private Point3D? _Location;
        public Point3D? Location
        {
            get => _Location;
            set
            {
                _Location = value;
                OnPropertyChanged();
            }
        }

        private double runSpeed;
        public double RunSpeed
        {
            get => runSpeed;
            set
            {
                runSpeed = value;
                OnPropertyChanged();
            }
        }

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
        
        public void StartCastingSpell(Spell spell, DateTime startedCastingOnDateTime)
        {
            appDispatcher.DispatchUI(() =>
            {
                UserCastingSpell = spell;
                UserCastSpellDateTime = startedCastingOnDateTime;
            });
        }
        
        public void FinishUserCastingSpell()
        {
            if (UserCastingSpell == null)
            {
                return;
            }
            
            // Determine if we should let the currently casting spell remain to give us a buffer to parse additional AoE lines, or to clear it now
            switch (_UserCastingSpell.SpellType)
            {
                case SpellType.PointBlankAreaofEffect:
                case SpellType.TargetedAreaofEffect:
                case SpellType.GroupV1:
                case SpellType.Groupv2:
                    ClearUserCastingSpellLater(_UserCastingSpell, _UserCastSpellDateTime);
                    break;
                default:
                    ClearUserCastingSpellImmediately();
                    break;
            }
        }

        public void ClearUserCastingSpellLater(Spell spell, DateTime? castedOn)
        {
            appDispatcher.DebounceToUI(ref aoeDebounceTs, 200, ClearUserCastingSpellImmediately, shouldCancel: () =>
            {
                var castTimeDiff = Math.Abs(((castedOn ?? DateTime.MaxValue) - (UserCastSpellDateTime ?? DateTime.MinValue)).TotalMilliseconds);
                return UserCastingSpell != spell || castTimeDiff > 200;
            });
        }

        public void ClearUserCastingSpellImmediately()
        {
            appDispatcher.DispatchUI(() =>
            {
                aoeDebounceTs?.Cancel();
                UserCastingSpell = null;
                UserCastSpellDateTime = null;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
