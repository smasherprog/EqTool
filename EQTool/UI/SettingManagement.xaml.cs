using EQTool.Models;
using EQTool.Services;
using EQTool.UI.SettingsComponents;
using EQTool.ViewModels;

namespace EQTool.UI
{
    public partial class SettingManagement : BaseSaveStateWindow
    {
        public SettingManagement(
            SettingsGeneral settingsGeneral,
            EQToolSettingsLoad toolSettingsLoad,
            EQToolSettings settings,
            ConsoleViewModel consoleViewModel) : base(settings.SettingsWindowState, toolSettingsLoad, settings, consoleViewModel)
        {
            InitializeComponent();
            MainContent.Content = settingsGeneral;
            base.Init();
        }
    }
}
