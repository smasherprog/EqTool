using EQTool.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace EQTool.Models
{
    public class TriggerViewModel : INotifyPropertyChanged
    {
        private readonly Trigger _Newtrigger;
        private readonly Trigger _OldTrigger = new Trigger();
        private readonly EQToolSettings toolSettings;
        private readonly EQToolSettingsLoad settingsLoad;
        private bool IsNewTrigger = false;

        public TriggerViewModel(Trigger trigger, EQToolSettings toolSettings, EQToolSettingsLoad settingsLoad)
        {
            this._Newtrigger = trigger;
            this.toolSettings = toolSettings;
            this.settingsLoad = settingsLoad;
            this.Copy(_Newtrigger, this._OldTrigger);
        }

        public TriggerViewModel(EQToolSettings toolSettings, EQToolSettingsLoad settingsLoad)
        {
            IsNewTrigger = true;
            this._Newtrigger = new Trigger();
            this._Newtrigger.TriggerEnabled = false;
            this._Newtrigger.TriggerId = Guid.NewGuid();
            this._Newtrigger.TriggerName = "New Trigger";
            this._Newtrigger.SearchText = string.Empty;
            this.toolSettings = toolSettings;
            this.settingsLoad = settingsLoad;
            this.Copy(_Newtrigger, this._OldTrigger);
        }

        public void Save()
        {
            this.Copy(this._OldTrigger, this._Newtrigger);
            if (this.IsNewTrigger)
            {
                this.toolSettings.Triggers.Add(this._Newtrigger);
            }
            this.settingsLoad.Save(toolSettings);
            this.OnPropertyChanged(nameof(IsDirty));
        }

        private void Copy(Trigger src, Trigger dst)
        {
            this.TriggerId = src.TriggerId;
            this.TriggerName = src.TriggerName;
            this.AudioText = src.AudioText;
            this.AudioTextEnabled = src.AudioTextEnabled;
            this.DisplayText = src.DisplayText;
            this.DisplayTextEnabled = src.DisplayTextEnabled;
            this.SearchText = src.SearchText;
            this.TriggerEnabled = src.TriggerEnabled;
        }

        public bool IsSavable
        {
            get
            {
                return
                    string.IsNullOrWhiteSpace(this._TriggerNameErrorMessge) &&
                     string.IsNullOrWhiteSpace(this._SearchTextErrorMessge) &&
                      string.IsNullOrWhiteSpace(this._AudioTextErrorMessge);
            }
        }

        public Visibility IsDirty
        {
            get
            {
                return
                    (this._OldTrigger.TriggerId != this._Newtrigger.TriggerId ||
                    this._OldTrigger.TriggerName != this._Newtrigger.TriggerName ||
                    this._OldTrigger.AudioText != this._Newtrigger.AudioText ||
                    this._OldTrigger.AudioTextEnabled != this._Newtrigger.AudioTextEnabled ||
                    this._OldTrigger.DisplayText != this._Newtrigger.DisplayText ||
                    this._OldTrigger.DisplayTextEnabled != this._Newtrigger.DisplayTextEnabled ||
                    this._OldTrigger.SearchText != this._Newtrigger.SearchText ||
                    this._OldTrigger.TriggerEnabled != this._Newtrigger.TriggerEnabled) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Guid TriggerId
        {
            get { return this._OldTrigger.TriggerId; }
            set
            {
                this._OldTrigger.TriggerId = value;
                this.OnPropertyChanged();
            }
        }

        public bool TriggerEnabled
        {
            get { return this._OldTrigger.TriggerEnabled; }
            set
            {
                this._OldTrigger.TriggerEnabled = value;
                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }

        public Brush TriggerNameBorderBrush
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._TriggerNameErrorMessge))
                {
                    return DefaultBrush;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }

        public Visibility TriggerNameErrorMessgeVisible
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._TriggerNameErrorMessge))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private string _TriggerNameErrorMessge = string.Empty;
        public string TriggerNameErrorMessge
        {
            get { return _TriggerNameErrorMessge; }
            set
            {
                if (_TriggerNameErrorMessge != value)
                {
                    _TriggerNameErrorMessge = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(TriggerNameErrorMessgeVisible));
                    this.OnPropertyChanged(nameof(TriggerNameBorderBrush));
                    this.OnPropertyChanged(nameof(IsSavable));
                }
            }
        }

        public string TriggerName
        {
            get { return this._OldTrigger.TriggerName; }
            set
            {
                this._OldTrigger.TriggerName = value;
                if (string.IsNullOrWhiteSpace(_OldTrigger.TriggerName) || _OldTrigger.TriggerName.Length < 4)
                {
                    TriggerNameErrorMessge = "Trigger Name must be at least 4 characters long!";
                }
                else
                {
                    TriggerNameErrorMessge = string.Empty;
                }
                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }
        private static Brush DefaultBrush
        {
            get
            {
                return Brushes.Gray;
            }
        }
        public Brush SearchTextBorderBrush
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._SearchTextErrorMessge))
                {
                    return DefaultBrush;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }

        public Visibility SearchTextErrorMessgeVisible
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._SearchTextErrorMessge))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private string _SearchTextErrorMessge = string.Empty;
        public string SearchTextErrorMessge
        {
            get { return _SearchTextErrorMessge; }
            set
            {
                if (_SearchTextErrorMessge != value)
                {
                    _SearchTextErrorMessge = value;

                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(SearchTextErrorMessgeVisible));
                    this.OnPropertyChanged(nameof(SearchTextBorderBrush));
                    this.OnPropertyChanged(nameof(IsSavable));
                }
            }
        }

        public string SearchText
        {
            get { return this._OldTrigger.SearchText; }
            set
            {
                this._OldTrigger.SearchText = value;
                if (string.IsNullOrWhiteSpace(_OldTrigger.SearchText) || _OldTrigger.SearchText.Length < 4)
                {
                    SearchTextErrorMessge = "Regex must be at least 4 characters long!";
                }
                else
                {
                    try
                    {
                        var r = this._OldTrigger.TriggerRegex;
                        if (r == null)
                        {
                            this.SearchTextErrorMessge = "There was an error creating the regex!";
                        }
                        else
                        {
                            this.SearchTextErrorMessge = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.SearchTextErrorMessge = ex.Message;
                    }
                }

                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }

        public Brush DisplayTextBorderBrush
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._DisplayTextErrorMessge))
                {
                    return DefaultBrush;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }

        public Visibility DisplayTextErrorMessgeVisible
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._DisplayTextErrorMessge))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private string _DisplayTextErrorMessge = string.Empty;
        public string DisplayTextErrorMessge
        {
            get { return _DisplayTextErrorMessge; }
            set
            {
                if (_DisplayTextErrorMessge != value)
                {
                    _DisplayTextErrorMessge = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(DisplayTextErrorMessgeVisible));
                    this.OnPropertyChanged(nameof(DisplayTextBorderBrush));
                    this.OnPropertyChanged(nameof(IsSavable));
                }
            }
        }

        public string DisplayText
        {
            get { return this._OldTrigger.DisplayText; }
            set
            {
                this._OldTrigger.DisplayText = value;
                if (string.IsNullOrWhiteSpace(_OldTrigger.DisplayText) || _OldTrigger.DisplayText.Length < 4)
                {
                    DisplayTextErrorMessge = "DisplayText be at least 4 characters long!";
                }
                else
                {
                    DisplayTextErrorMessge = string.Empty;
                }
                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }

        public bool DisplayTextEnabled
        {
            get { return this._OldTrigger.DisplayTextEnabled; }
            set
            {
                this._OldTrigger.DisplayTextEnabled = value;
                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }

        public bool AudioTextEnabled
        {
            get { return this._OldTrigger.AudioTextEnabled; }
            set
            {
                this._OldTrigger.AudioTextEnabled = value;
                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }

        public Brush AudioTextBorderBrush
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._AudioTextErrorMessge))
                {
                    return DefaultBrush;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }

        public Visibility AudioTextErrorMessgeVisible
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._AudioTextErrorMessge))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private string _AudioTextErrorMessge = string.Empty;
        public string AudioTextErrorMessge
        {
            get { return _AudioTextErrorMessge; }
            set
            {
                if (_AudioTextErrorMessge != value)
                {
                    _AudioTextErrorMessge = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(AudioTextErrorMessgeVisible));
                    this.OnPropertyChanged(nameof(AudioTextBorderBrush));
                    this.OnPropertyChanged(nameof(IsSavable));
                }
            }
        }

        public string AudioText
        {
            get { return this._OldTrigger.AudioText; }
            set
            {
                this._OldTrigger.AudioText = value;
                if (string.IsNullOrWhiteSpace(_OldTrigger.AudioText) || _OldTrigger.AudioText.Length < 4)
                {
                    AudioTextErrorMessge = "AudioText be at least 4 characters long!";
                }
                else
                {
                    AudioTextErrorMessge = string.Empty;
                }
                this.OnPropertyChanged(nameof(IsDirty));
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}