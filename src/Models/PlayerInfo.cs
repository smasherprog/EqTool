using EQToolShared.Enums;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EQTool.Models
{
    public class SessionPlayerDamage : INotifyPropertyChanged
    {
        private PlayerDamage _CurrentSessionPlayerDamage;
        public PlayerDamage CurrentSessionPlayerDamage
        {
            get
            {
                if (_CurrentSessionPlayerDamage == null)
                {
                    _CurrentSessionPlayerDamage = new PlayerDamage();
                }
                return _CurrentSessionPlayerDamage;
            }
            set
            {
                _CurrentSessionPlayerDamage = value ?? new PlayerDamage();
                OnPropertyChanged();
            }
        }

        public Visibility LastSessionPlayerDamageVisability => _LastSessionPlayerDamage != null ? Visibility.Visible : Visibility.Collapsed;

        private PlayerDamage _LastSessionPlayerDamage;
        public PlayerDamage LastSessionPlayerDamage
        {
            get => _LastSessionPlayerDamage;
            set
            {
                _LastSessionPlayerDamage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastSessionPlayerDamageVisability));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    [Serializable]
    public class PlayerDamage : INotifyPropertyChanged
    {
        private int _TargetTotalDamage;
        public int TargetTotalDamage
        {
            get => _TargetTotalDamage;
            set
            {
                _TargetTotalDamage = value;
                OnPropertyChanged();
            }
        }

        private int _HighestDPS = 0;
        public int HighestDPS
        {
            get => _HighestDPS;
            set
            {
                _HighestDPS = value;
                OnPropertyChanged();
            }
        }

        private int _HighestHit = 0;
        public int HighestHit
        {
            get => _HighestHit;
            set
            {
                if (_HighestHit >= 32000)
                {
                    _HighestHit = 0;
                }

                if (value >= 32000)
                {
                    return;
                }

                _HighestHit = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class YouSpells
    {
        public string Name { get; set; }
        public int TotalSecondsLeft { get; set; }
    }

    [Serializable]
    public class PlayerInfo : INotifyPropertyChanged
    {
        private int _Level = 1;
        public int Level
        {
            get => _Level;
            set
            {
                _Level = value;
                OnPropertyChanged();
            }
        }

        private PlayerDamage _BestPlayerDamage;

        public PlayerDamage BestPlayerDamage
        {
            get
            {
                if (_BestPlayerDamage == null)
                {
                    _BestPlayerDamage = new PlayerDamage();
                }
                return _BestPlayerDamage;
            }
            set
            {
                _BestPlayerDamage = value ?? new PlayerDamage();
                OnPropertyChanged();
            }
        }

        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged();
            }
        }

        private int? _TrackingSkill;
        public int? TrackingSkill
        {
            get => _TrackingSkill;
            set
            {
                _TrackingSkill = value;
                OnPropertyChanged();
            }
        }

        private string _GuildName;
        public string GuildName
        {
            get => _GuildName;
            set
            {
                _GuildName = value;
                OnPropertyChanged();
            }
        }

        private string _Zone;
        public string Zone
        {
            get => _Zone;
            set
            {
                _Zone = value;
                OnPropertyChanged();
            }
        }

        private bool _SpellDebuffShare;
        public bool SpellDebuffShare
        {
            get => _SpellDebuffShare;
            set
            {
                SpellDebuffShare = value;
                OnPropertyChanged();
            }
        }

        private MapLocationSharing _MapLocationSharing = EQToolShared.Map.MapLocationSharing.Everyone;
        public MapLocationSharing MapLocationSharing
        {
            get => _MapLocationSharing;
            set
            {
                _MapLocationSharing = value;
                OnPropertyChanged();
            }
        }

        private bool _MapKillTimers = true;
        public bool? MapKillTimers
        {
            get => _MapKillTimers;
            set
            {
                _MapKillTimers = value ?? true;
                OnPropertyChanged();
            }
        }

        private PlayerClasses? _PlayerClass;
        public PlayerClasses? PlayerClass
        {
            get => _PlayerClass;
            set
            {
                var istrackabkebefore = IsTrackableClass;
                _PlayerClass = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTrackableClass));
                if (istrackabkebefore && !IsTrackableClass)
                {
                    TrackingSkill = null;
                }
            }
        }
        public double? TrackingDistance
        {
            get
            {
                var pclass = PlayerClass;
                if (pclass.HasValue)
                {
                    var trackskill = this.TrackingSkill ?? 10;
                    if (pclass == EQToolShared.Enums.PlayerClasses.Ranger)
                    {
                        return (trackskill * 12) * 2;
                    }
                    else if (pclass == EQToolShared.Enums.PlayerClasses.Druid)
                    {
                        return (trackskill * 10) * 2;
                    }
                    else if (pclass == EQToolShared.Enums.PlayerClasses.Bard)
                    {
                        return (trackskill * 7) * 2;
                    }
                }
                return null;
            }
        }

        public bool IsTrackableClass => PlayerClass == EQToolShared.Enums.PlayerClasses.Druid || PlayerClass == EQToolShared.Enums.PlayerClasses.Ranger || PlayerClass == EQToolShared.Enums.PlayerClasses.Bard;

        private Servers? _Server;
        public Servers? Server
        {
            get => _Server;
            set
            {
                _Server = value;
                OnPropertyChanged();
            }
        }

        private List<YouSpells> _YouSpells;

        public List<YouSpells> YouSpells
        {
            get
            {
                if (_YouSpells == null)
                {
                    _YouSpells = new List<YouSpells>();
                }
                return _YouSpells;
            }
            set
            {
                _YouSpells = value ?? new List<YouSpells>();
                OnPropertyChanged();
            }
        }

        public List<PlayerClasses> ShowSpellsForClasses { get; set; }

        private bool _DragonRoarOverlay;
        public bool DragonRoarOverlay
        {
            get => _DragonRoarOverlay;
            set
            {
                _DragonRoarOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _DragonRoarAudio;
        public bool DragonRoarAudio
        {
            get => _DragonRoarAudio;
            set
            {
                _DragonRoarAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _GroupInviteOverlay;
        public bool GroupInviteOverlay
        {
            get => _GroupInviteOverlay;
            set
            {
                _GroupInviteOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _GroupInviteAudio;
        public bool GroupInviteAudio
        {
            get => _GroupInviteAudio;
            set
            {
                _GroupInviteAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _FailedFeignOverlay;
        public bool FailedFeignOverlay
        {
            get => _FailedFeignOverlay;
            set
            {
                _FailedFeignOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _FailedFeignAudio;
        public bool FailedFeignAudio
        {
            get => _FailedFeignAudio;
            set
            {
                _FailedFeignAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _EnrageOverlay;
        public bool EnrageOverlay
        {
            get => _EnrageOverlay;
            set
            {
                _EnrageOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _EnrageAudio;
        public bool EnrageAudio
        {
            get => _EnrageAudio;
            set
            {
                _EnrageAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _LevFadingAudio;
        public bool LevFadingAudio
        {
            get => _LevFadingAudio;
            set
            {
                _LevFadingAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _LevFadingOverlay;
        public bool LevFadingOverlay
        {
            get => _LevFadingOverlay;
            set
            {
                _LevFadingOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _CharmBreakAudio;
        public bool CharmBreakAudio
        {
            get => _CharmBreakAudio;
            set
            {
                _CharmBreakAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _CharmBreakOverlay;
        public bool CharmBreakOverlay
        {
            get => _CharmBreakOverlay;
            set
            {
                _CharmBreakOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _FTEAudio;
        public bool FTEAudio
        {
            get => _FTEAudio;
            set
            {
                _FTEAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _FTEOverlay;
        public bool FTEOverlay
        {
            get => _FTEOverlay;
            set
            {
                _FTEOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _InvisFadingAudio;
        public bool InvisFadingAudio
        {
            get => _InvisFadingAudio;
            set
            {
                _InvisFadingAudio = value;
                OnPropertyChanged();
            }
        }

        private bool _InvisFadingOverlay;
        public bool InvisFadingOverlay
        {
            get => _InvisFadingOverlay;
            set
            {
                _InvisFadingOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _ChChainWarningOverlay;
        public bool ChChainWarningOverlay
        {
            get => _ChChainWarningOverlay;
            set
            {
                _ChChainWarningOverlay = value;
                if (_ChChainWarningOverlay)
                {
                    _ChChainOverlay = true;
                    OnPropertyChanged(nameof(ChChainOverlay));
                }
                OnPropertyChanged();
            }
        }

        private bool _ChChainWarningAudio;
        public bool ChChainWarningAudio
        {
            get => _ChChainWarningAudio;
            set
            {
                _ChChainWarningAudio = value;
                if (_ChChainWarningAudio)
                {
                    _ChChainOverlay = true;
                    OnPropertyChanged(nameof(ChChainOverlay));
                }
                OnPropertyChanged();
            }
        }

        private bool _ChChainOverlay;
        public bool ChChainOverlay
        {
            get => _ChChainOverlay;
            set
            {
                _ChChainOverlay = value;
                if (!_ChChainOverlay)
                {
                    _ChChainWarningAudio = false;
                    OnPropertyChanged(nameof(ChChainWarningAudio));
                    _ChChainWarningOverlay = false;
                    OnPropertyChanged(nameof(ChChainWarningOverlay));
                }
                OnPropertyChanged();
            }
        }

        private string _ChChainTagOverlay;
        public string ChChainTagOverlay
        {
            get => _ChChainTagOverlay;
            set
            {
                _ChChainTagOverlay = value;
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
