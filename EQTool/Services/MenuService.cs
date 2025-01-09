namespace EQTool.Services
{
    public class MenuService
    {
        public readonly System.Windows.Forms.NotifyIcon SystemTrayIcon;
        public readonly System.Windows.Forms.MenuItem MapMenuItem;
        public readonly System.Windows.Forms.MenuItem SpellsMenuItem;
        public readonly System.Windows.Forms.MenuItem DpsMeterMenuItem;
        public readonly System.Windows.Forms.MenuItem OverlayMenuItem;
        public readonly System.Windows.Forms.MenuItem SettingsMenuItem;
        public readonly System.Windows.Forms.MenuItem MobInfoMenuItem;

        public MenuService()
        {

            SettingsMenuItem = new System.Windows.Forms.MenuItem("Settings", ToggleSettingsWindow);
            SpellsMenuItem = new System.Windows.Forms.MenuItem("Triggers", ToggleSpellsWindow);
            MapMenuItem = new System.Windows.Forms.MenuItem("Map", ToggleMapWindow);
            DpsMeterMenuItem = new System.Windows.Forms.MenuItem("Dps", ToggleDPSWindow);
            OverlayMenuItem = new System.Windows.Forms.MenuItem("Overlay", ToggleOverlayWindow);
            MobInfoMenuItem = new System.Windows.Forms.MenuItem("Mob Info", ToggleMobInfoWindow);
            var gitHubMenuItem = new System.Windows.Forms.MenuItem("Suggestions", Suggestions);
            var whythepig = new System.Windows.Forms.MenuItem("Pigparse Discord", WhyThePig);
            var updates = new System.Windows.Forms.MenuItem("Check for Update", CheckForUpdates);
            var logo = EQTool.Properties.Resources.pig;

#if BETA || DEBUG
            logo = EQTool.Properties.Resources.sickpic;
#endif

            var version = new System.Windows.Forms.MenuItem(AppRoot.Version)
            {
                Enabled = false
            };
            ToggleMenuButtons(false);
            SystemTrayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = logo,
                Visible = true,
                ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]
                {
                    //GroupSuggestionsMenuItem,
                    whythepig,
                    OverlayMenuItem,
                    DpsMeterMenuItem,
                    MapMenuItem,
                    SpellsMenuItem,
                    MobInfoMenuItem,
                    SettingsMenuItem,
                    gitHubMenuItem,
                    updates,
                    version,
                    new System.Windows.Forms.MenuItem("Exit", OnExit)
                }),
            };
        }

    }
}
