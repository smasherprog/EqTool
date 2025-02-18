using EQTool.Models;
using EQTool.ViewModels;

namespace EQTool.Services.Handlers
{
    public class GroupLeaderHandler : BaseHandler
    {
        // reference to DI global
        private readonly SettingsWindowViewModel settingsWindowViewModel;

        // ctor
        public GroupLeaderHandler(BaseHandlerData baseHandlerData, SettingsWindowViewModel settingsWindowViewModel) : base(baseHandlerData)
        {
            this.settingsWindowViewModel = settingsWindowViewModel;


            logEvents.GroupLeaderEvent += LogEvents_GroupLeaderEvent;
            logEvents.WelcomeEvent += LogEvents_WelcomeEvent;
        }

        private void LogEvents_GroupLeaderEvent(object sender, GroupLeaderEvent e)
        {
            settingsWindowViewModel.GroupLeaderName = e.GroupLeaderName;
        }

        // ensure group leader name is cleared upon login
        private void LogEvents_WelcomeEvent(object sender, WelcomeEvent e)
        {
            settingsWindowViewModel.GroupLeaderName = "None";
        }

    }
}
