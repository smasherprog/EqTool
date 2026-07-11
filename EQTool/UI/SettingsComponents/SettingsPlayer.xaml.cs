using EQTool.Models;
using EQTool.ViewModels.SettingsComponents;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;

namespace EQTool.UI.SettingsComponents
{
    /// <summary>
    /// Right-hand panel of the Characters tab: shows the selected character's
    /// inventory profile by embedding the pigparse HTML view in a WebBrowser.
    /// </summary>
    public partial class SettingsPlayer : UserControl
    {
        static SettingsPlayer()
        {
            EnsureBrowserEmulation();
        }

        public SettingsPlayer(TreePlayer treePlayer, EQToolSettings settings)
        {
            DataContext = new SettingsPlayerViewModel(treePlayer);
            InitializeComponent();
            ProfileBrowser.Navigated += (s, e) => SetSilent(ProfileBrowser);

            var characterName = treePlayer?.Player?.Name;
            if (string.IsNullOrWhiteSpace(characterName))
            {
                ProfileBrowser.NavigateToString(MessageDocument("No character selected."));
            }
            else if (string.IsNullOrWhiteSpace(settings?.DiscordApiToken))
            {
                ProfileBrowser.NavigateToString(MessageDocument($"Log in with Discord on the General tab to view {characterName}'s inventory."));
            }
            else
            {
                var url = $"https://pigparse.azurewebsites.net/api/inventory/profile?character={Uri.EscapeDataString(characterName)}";
                ProfileBrowser.Navigate(new Uri(url), null, null, $"Authorization: Bearer {settings.DiscordApiToken}\r\n");
            }
        }

        private static string MessageDocument(string message)
        {
            var encoded = System.Net.WebUtility.HtmlEncode(message);
            return "<!DOCTYPE html><html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"/></head>"
                + "<body style=\"background:#0b0e1a;color:#9aa3c0;font-family:'Segoe UI',Arial,sans-serif;font-size:14px;text-align:center;padding:60px 20px;\">"
                + encoded + "</body></html>";
        }

        // The WPF WebBrowser control renders in IE7 mode unless the exe opts into
        // IE11 via this HKCU (no admin needed) feature key. Without it the profile
        // page's CSS does not render correctly.
        private static void EnsureBrowserEmulation()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    var exe = Process.GetCurrentProcess().MainModule?.ModuleName;
                    if (!string.IsNullOrEmpty(exe))
                    {
                        key?.SetValue(exe, 11001, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        // Suppress IE script-error dialog popups inside the embedded browser.
        private static void SetSilent(WebBrowser browser)
        {
            try
            {
                var activeXProp = typeof(WebBrowser).GetProperty("ActiveXInstance", BindingFlags.Instance | BindingFlags.NonPublic);
                var activeX = activeXProp?.GetValue(browser, null);
                _ = (activeX?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, activeX, new object[] { true }));
            }
            catch { }
        }
    }
}
