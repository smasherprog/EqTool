using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels;
using EQTool.ViewModels.MobInfoComponents;
using EQToolShared.Enums;

namespace EQTool.UI
{
    public partial class MobInfo : BaseSaveStateWindow
    {
        public MobInfo(IAppDispatcher appDispatcher, MobInfoManagementViewModel mobInfoViewModel, EQToolSettings settings, EQToolSettingsLoad toolSettingsLoad, LoggingService loggingService, ActivePlayer activePlayer)
            : base(appDispatcher, mobInfoViewModel, settings.MobWindowState, toolSettingsLoad, settings)
        {
            loggingService.Log(string.Empty, EventType.OpenMobInfo, activePlayer?.Player?.Server);
            DataContext = mobInfoViewModel;
            InitializeComponent();
            base.Init();
        }
    }
}
