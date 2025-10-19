﻿using EQTool.Services;
using EQTool.ViewModels.SettingsComponents;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EQTool.ViewModels.MobInfoComponents
{
    public class MobInfoManagementViewModel : BaseWindowViewModel, INotifyPropertyChanged
    {
        public string Title { get; set; } = "Mob Info v" + App.Version;
        private readonly UserComponentSettingsManagementFactory userComponentFactory;
        public MobInfoManagementViewModel(UserComponentSettingsManagementFactory userComponentFactory)
        {
            this.userComponentFactory = userComponentFactory;
        }

        private MobInfoItemType _MobInfoItemType = SettingsComponents.MobInfoItemType.Mob;
        public MobInfoItemType MobInfoItemType
        {
            set
            {
                if (value != _MobInfoItemType)
                {
                    _MobInfoItemType = value;
                    UserControl = userComponentFactory.CreateComponent(_MobInfoItemType);
                }
            }
        }

        private UserControl _userControl;

        public UserControl UserControl
        {
            get
            {
                if (_userControl == null)
                {
                    _userControl = userComponentFactory.CreateComponent(_MobInfoItemType);
                }

                return _userControl;
            }
            set
            {
                _userControl = value;
                OnPropertyChanged();
            }
        }
    }
}
