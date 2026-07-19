using EQTool.Models;
using EQTool.Services;
using EQTool.ViewModels.SettingsComponents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EQTool.UI.SettingsComponents
{
    // DTOs matching EQToolApis api/inventory/profile-data JSON.
    public class ProfileItemDto
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Tooltip { get; set; }
        public int Count { get; set; }
        public int Capacity { get; set; }
        public int ItemId { get; set; }
    }

    public class ProfileBagDto
    {
        public ProfileItemDto Container { get; set; }
        public List<ProfileItemDto> Contents { get; set; }
    }

    public class ProfileStatsDto
    {
        public int AC { get; set; }
        public int HP { get; set; }
        public int Mana { get; set; }
        public int Atk { get; set; }
        public int Str { get; set; }
        public int Sta { get; set; }
        public int Agi { get; set; }
        public int Dex { get; set; }
        public int Wis { get; set; }
        public int Int { get; set; }
        public int Cha { get; set; }
        public int SvPoison { get; set; }
        public int SvMagic { get; set; }
        public int SvDisease { get; set; }
        public int SvFire { get; set; }
        public int SvCold { get; set; }
        public int Haste { get; set; }
        public double Weight { get; set; }
    }

    public class CharacterProfileDto
    {
        public string CharacterName { get; set; }
        public EQToolShared.Enums.Servers Server { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Dictionary<string, ProfileItemDto> Equipped { get; set; }
        public List<ProfileBagDto> General { get; set; }
        public List<ProfileBagDto> Bank { get; set; }
        public List<ProfileItemDto> SharedBank { get; set; }
        public ProfileStatsDto Stats { get; set; }
    }

    /// <summary>
    /// Right-hand panel of the Characters tab: shows the selected character's
    /// inventory profile, rendered natively in WPF from the pigparse JSON endpoint.
    /// (The settings window is a layered/transparent window, so an embedded
    /// WebBrowser/WebView cannot render here.) When the user is not logged into
    /// Discord (or has no uploaded data yet) it shows an explanation instead,
    /// with a login button when one is needed.
    /// </summary>
    public partial class SettingsPlayer : UserControl
    {
        private const string PigparseBase = "https://pigparse.azurewebsites.net";
        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly Brush PanelBg = MakeBrush("#FF131829");
        private static readonly Brush PanelBorder = MakeBrush("#FF242C49");
        private static readonly Brush SlotBg = MakeBrush("#FF0D1120");
        private static readonly Brush SlotBorder = MakeBrush("#FF262E4A");
        private static readonly Brush BadgeBg = MakeBrush("#FF1E2745");
        private static readonly Brush TextMain = MakeBrush("#FFCDD3E6");
        private static readonly Brush TextLabel = MakeBrush("#FF9AA3C0");
        private static readonly Brush TextMuted = MakeBrush("#FF6B7390");
        private static readonly Brush TextDim = MakeBrush("#FF565E7C");
        private static readonly Brush StatGreen = MakeBrush("#FF4ADE80");
        private static readonly Brush StatRed = MakeBrush("#FFF87171");
        private static readonly Brush TooltipBg = MakeBrush("#FF0C101F");
        private static readonly Brush TooltipBorder = MakeBrush("#FF3C4670");
        private static readonly Brush TooltipText = MakeBrush("#FFDFE4F5");

        // In-game inventory grid: a uniform 5-column layout mirroring the EQ
        // inventory window - ears/neck/head/face across the top, rings bookending
        // row 2, wrists bookending row 3, and the weapon row centered at the
        // bottom. Empty strings are spacer cells that render as blank slots so the
        // columns stay aligned. Held (cursor) is intentionally not displayed.
        // Keep this in sync with CharacterProfileHtml.EquipRows (the website copy).
        private static readonly string[][] EquipRows = new string[][]
        {
            new[] { "Ear1", "Neck", "Head", "Face", "Ear2" },
            new[] { "Finger1", "Shoulders", "Arms", "Back", "Finger2" },
            new[] { "Wrist1", "Chest", "Waist", "Legs", "Wrist2" },
            new[] { "Charm", "Hands", "Feet", "", "" },
            new[] { "Primary", "Secondary", "Range", "Ammo" },
        };

        private readonly EQToolSettings settings;
        private readonly EQToolSettingsLoad settingsLoad;
        private readonly string characterName;
        private readonly EQToolShared.Enums.Servers server;
        private TextBlock invTab;
        private TextBlock bankTab;
        private UIElement invPane;
        private UIElement bankPane;

        public SettingsPlayer(TreePlayer treePlayer, EQToolSettings settings, EQToolSettingsLoad settingsLoad)
        {
            this.settings = settings;
            this.settingsLoad = settingsLoad;
            characterName = treePlayer?.Player?.Name;
            server = treePlayer?.Player?.Server ?? EQToolShared.Enums.Servers.Green;
            DataContext = new SettingsPlayerViewModel(treePlayer);
            InitializeComponent();
            Refresh();
        }

        private static Brush MakeBrush(string hex)
        {
            var brush = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        private void Refresh()
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                ShowMessage("No character selected.", showLoginButton: false);
            }
            else if (string.IsNullOrWhiteSpace(settings?.DiscordApiToken))
            {
                ShowMessage(
                    "Log in with Discord to unlock character profiles!\n\n" +
                    $"Once logged in, Pigparse automatically uploads your inventory when you run /outputfile inventory in game, and shows {characterName}'s equipped gear, item stats, weight and bank right here.",
                    showLoginButton: true);
            }
            else
            {
                LoadProfile();
            }
        }

        private async void LoadProfile()
        {
            ShowMessage($"Loading {characterName}'s profile...", showLoginButton: false);
            try
            {
                var url = $"{PigparseBase}/api/inventory/profile-data?character={Uri.EscapeDataString(characterName)}&server={server}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.DiscordApiToken);
                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var profile = JsonConvert.DeserializeObject<CharacterProfileDto>(json);
                    if (profile == null)
                    {
                        ShowMessage($"Could not read {characterName}'s profile data. Try again later.", showLoginButton: false);
                        return;
                    }
                    RenderProfile(profile);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // The endpoint marks a real "no data" 404 with a body; a bare
                    // 404 means the server build does not have this endpoint yet.
                    var body = string.Empty;
                    try { body = await response.Content.ReadAsStringAsync(); } catch { }
                    if (body != null && body.Contains("no_inventory"))
                    {
                        ShowMessage(
                            $"No character data for {characterName} on {server} yet.\n\n" +
                            "Run /outputfile inventory in game while Pigparse is running and the inventory will upload automatically. Come back here to see equipped gear, item stats, weight and bank contents.",
                            showLoginButton: false);
                    }
                    else
                    {
                        ShowMessage("The Pigparse server does not support character profiles yet. Try again after the site has been updated.", showLoginButton: false);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    ShowMessage(
                        "Your Discord login has expired.\n\nLog in with Discord again to view character profiles with equipped gear, item stats and bank contents.",
                        showLoginButton: true);
                }
                else
                {
                    ShowMessage($"Could not load {characterName}'s profile (server returned {(int)response.StatusCode}). Try again later.", showLoginButton: false);
                }
            }
            catch
            {
                ShowMessage("Could not reach Pigparse. Check your internet connection and try again.", showLoginButton: false);
            }
        }

        private void ShowMessage(string message, bool showLoginButton)
        {
            ProfileScroll.Visibility = Visibility.Collapsed;
            MessagePanel.Visibility = Visibility.Visible;
            MessageText.Text = message;
            LoginButton.Visibility = showLoginButton ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            LoginButton.Content = "Logging in...";
            var authService = new DiscordAuthService();
            _ = authService.LoginAsync().ContinueWith(t =>
            {
                var result = t.Exception == null ? t.Result : null;
                _ = Dispatcher.BeginInvoke((Action)(() =>
                {
                    LoginButton.IsEnabled = true;
                    LoginButton.Content = "Login with Discord";
                    if (!string.IsNullOrEmpty(result?.ApiToken))
                    {
                        settings.DiscordUsername = result.Username;
                        settings.DiscordId = result.DiscordId;
                        settings.DiscordApiToken = result.ApiToken;
                        settingsLoad?.Save(settings);
                        Refresh();
                    }
                }));
            });
        }

        // ---- native profile rendering ----

        public void RenderProfile(CharacterProfileDto profile)
        {
            MessagePanel.Visibility = Visibility.Collapsed;
            ProfileScroll.Visibility = Visibility.Visible;
            ProfileHost.Children.Clear();

            var statsPanel = BuildStatsPanel(profile);
            Grid.SetColumn(statsPanel, 0);
            _ = ProfileHost.Children.Add(statsPanel);

            var main = new StackPanel();
            main.Children.Add(BuildEquippedPanel(profile));
            main.Children.Add(BuildBagsPanel(profile));
            Grid.SetColumn(main, 1);
            _ = ProfileHost.Children.Add(main);
        }

        private FrameworkElement BuildStatsPanel(CharacterProfileDto profile)
        {
            var s = profile.Stats ?? new ProfileStatsDto();
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock { Text = profile.CharacterName, FontSize = 19, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
            stack.Children.Add(new TextBlock { Text = $"{profile.Server} · Updated {profile.UpdatedAt.ToLocalTime():d}", FontSize = 11, Foreground = TextMuted, Margin = new Thickness(0, 2, 0, 10) });

            var core = new UniformGrid { Columns = 4 };
            core.Children.Add(CoreCell("HP", s.HP == 0 ? "—" : "+" + s.HP));
            core.Children.Add(CoreCell("Mana", s.Mana == 0 ? "—" : "+" + s.Mana));
            core.Children.Add(CoreCell("AC", s.AC.ToString()));
            core.Children.Add(CoreCell("ATK", s.Atk == 0 ? "—" : "+" + s.Atk));
            stack.Children.Add(new Border
            {
                BorderBrush = PanelBorder,
                BorderThickness = new Thickness(0, 1, 0, 1),
                Padding = new Thickness(0, 8, 0, 8),
                Child = core
            });

            stack.Children.Add(SectionHeader("Attributes"));
            stack.Children.Add(BonusRow("STR", s.Str));
            stack.Children.Add(BonusRow("STA", s.Sta));
            stack.Children.Add(BonusRow("AGI", s.Agi));
            stack.Children.Add(BonusRow("DEX", s.Dex));
            stack.Children.Add(BonusRow("WIS", s.Wis));
            stack.Children.Add(BonusRow("INT", s.Int));
            stack.Children.Add(BonusRow("CHA", s.Cha));

            stack.Children.Add(SectionHeader("Resists"));
            stack.Children.Add(BonusRow("Poison", s.SvPoison));
            stack.Children.Add(BonusRow("Magic", s.SvMagic));
            stack.Children.Add(BonusRow("Disease", s.SvDisease));
            stack.Children.Add(BonusRow("Fire", s.SvFire));
            stack.Children.Add(BonusRow("Cold", s.SvCold));

            stack.Children.Add(SectionHeader("Other"));
            stack.Children.Add(StatRow("Haste", s.Haste == 0 ? "—" : s.Haste + " %", s.Haste == 0 ? TextDim : StatGreen));
            stack.Children.Add(StatRow("Weight", s.Weight.ToString("0.#"), StatGreen));

            stack.Children.Add(new TextBlock { Text = "Totals from equipped item stats", FontSize = 10, Foreground = TextDim, Margin = new Thickness(0, 14, 0, 0) });

            return new Border
            {
                Background = PanelBg,
                BorderBrush = PanelBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 12, 12),
                Width = 250,
                VerticalAlignment = VerticalAlignment.Top,
                Child = stack
            };
        }

        private static UIElement CoreCell(string label, string value)
        {
            var sp = new StackPanel();
            sp.Children.Add(new TextBlock { Text = label, FontSize = 10, Foreground = TextMuted, HorizontalAlignment = HorizontalAlignment.Center });
            sp.Children.Add(new TextBlock { Text = value, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center });
            return sp;
        }

        private static TextBlock SectionHeader(string text)
        {
            return new TextBlock { Text = text, FontWeight = FontWeights.Bold, Foreground = Brushes.White, Margin = new Thickness(0, 12, 0, 3) };
        }

        private static UIElement StatRow(string label, string value, Brush valueBrush)
        {
            var dock = new DockPanel { Margin = new Thickness(8, 1, 0, 1) };
            var valueText = new TextBlock { Text = value, Foreground = valueBrush, FontWeight = FontWeights.Bold };
            DockPanel.SetDock(valueText, Dock.Right);
            dock.Children.Add(valueText);
            dock.Children.Add(new TextBlock { Text = label, Foreground = TextLabel });
            return dock;
        }

        private static UIElement BonusRow(string label, int value)
        {
            if (value == 0)
            {
                return StatRow(label, "—", TextDim);
            }
            return StatRow(label, (value > 0 ? "+" : string.Empty) + value, value < 0 ? StatRed : StatGreen);
        }

        private FrameworkElement BuildEquippedPanel(CharacterProfileDto profile)
        {
            var rows = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };
            foreach (var rowKeys in EquipRows)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                foreach (var key in rowKeys)
                {
                    ProfileItemDto item = null;
                    _ = profile.Equipped != null && profile.Equipped.TryGetValue(key, out item);
                    row.Children.Add(MakeSlot(item));
                }
                rows.Children.Add(row);
            }

            var content = new StackPanel();
            content.Children.Add(PanelHeader("Equipped Items"));
            content.Children.Add(rows);
            return MakePanel(content);
        }

        private FrameworkElement BuildBagsPanel(CharacterProfileDto profile)
        {
            invTab = MakeTab("Inventory");
            bankTab = MakeTab("Bank");
            invTab.MouseLeftButtonDown += (s, e) => ShowTab(inventory: true);
            bankTab.MouseLeftButtonDown += (s, e) => ShowTab(inventory: false);

            var tabRow = new StackPanel { Orientation = Orientation.Horizontal };
            tabRow.Children.Add(invTab);
            tabRow.Children.Add(bankTab);
            var tabHeader = new Border
            {
                BorderBrush = PanelBorder,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(14, 10, 14, 0),
                Child = tabRow
            };

            var invStack = new StackPanel { Margin = new Thickness(12) };
            invStack.Children.Add(MakeBagRows(profile.General, 4));
            invPane = invStack;

            var bankStack = new StackPanel { Margin = new Thickness(12), Visibility = Visibility.Collapsed };
            bankStack.Children.Add(MakeBagRows(profile.Bank, 4));
            var sharedRow = new StackPanel { Orientation = Orientation.Horizontal };
            if (profile.SharedBank != null)
            {
                foreach (var item in profile.SharedBank)
                {
                    sharedRow.Children.Add(MakeSlot(item));
                }
            }
            var shared = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            shared.Children.Add(new TextBlock { Text = "Shared Bank", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = TextMuted, Margin = new Thickness(2, 0, 0, 2) });
            shared.Children.Add(sharedRow);
            bankStack.Children.Add(shared);
            bankPane = bankStack;

            ShowTab(inventory: true);

            var content = new StackPanel();
            content.Children.Add(tabHeader);
            content.Children.Add(invStack);
            content.Children.Add(bankStack);
            return MakePanel(content);
        }

        // Bags render 4 across per row (inventory: 2 rows, bank: 4 rows).
        private FrameworkElement MakeBagRows(List<ProfileBagDto> bags, int perRow)
        {
            var rows = new StackPanel();
            if (bags == null)
            {
                return rows;
            }
            for (var start = 0; start < bags.Count; start += perRow)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                for (var i = start; i < Math.Min(start + perRow, bags.Count); i++)
                {
                    var group = MakeBagGroup(bags[i]);
                    group.Margin = new Thickness(0, 4, 24, 10);
                    row.Children.Add(group);
                }
                rows.Children.Add(row);
            }
            return rows;
        }

        private static TextBlock MakeTab(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 22, 10),
                Cursor = Cursors.Hand
            };
        }

        private void ShowTab(bool inventory)
        {
            if (invPane == null || bankPane == null)
            {
                return;
            }
            invPane.Visibility = inventory ? Visibility.Visible : Visibility.Collapsed;
            bankPane.Visibility = inventory ? Visibility.Collapsed : Visibility.Visible;
            invTab.Foreground = inventory ? Brushes.White : TextMuted;
            bankTab.Foreground = inventory ? TextMuted : Brushes.White;
        }

        private FrameworkElement MakeBagGroup(ProfileBagDto bag)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 4, 0, 10) };
            sp.Children.Add(MakeSlot(bag?.Container));
            var bagName = bag?.Container?.Name;
            if (!string.IsNullOrEmpty(bagName))
            {
                sp.Children.Add(new TextBlock
                {
                    Text = bagName,
                    Foreground = TextLabel,
                    FontSize = 11,
                    Width = 100,
                    Margin = new Thickness(0, 1, 0, 3),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
            }
            if (bag?.Contents != null && bag.Contents.Count > 0)
            {
                var grid = new UniformGrid { Columns = 2, HorizontalAlignment = HorizontalAlignment.Left };
                foreach (var item in bag.Contents)
                {
                    grid.Children.Add(MakeSlot(item));
                }
                sp.Children.Add(grid);
            }
            return sp;
        }

        private FrameworkElement MakeSlot(ProfileItemDto item)
        {
            var border = new Border
            {
                Width = 46,
                Height = 46,
                CornerRadius = new CornerRadius(6),
                Background = SlotBg,
                BorderBrush = SlotBorder,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            if (item == null)
            {
                return border;
            }

            var grid = new Grid();
            var img = new Image { Width = 44, Height = 44, Stretch = Stretch.Uniform };
            try
            {
                var url = !string.IsNullOrEmpty(item.Image) && item.Image.StartsWith("/") ? PigparseBase + item.Image : item.Image;
                if (!string.IsNullOrEmpty(url))
                {
                    img.Source = new BitmapImage(new Uri(url));
                }
            }
            catch { }
            grid.Children.Add(img);

            if (item.Count > 1)
            {
                grid.Children.Add(new Border
                {
                    Background = BadgeBg,
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(3, 0, 3, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 1, 1),
                    Child = new TextBlock { Text = item.Count.ToString(), Foreground = TextMain, FontSize = 10 }
                });
            }
            border.Child = grid;

            var tooltipText = string.IsNullOrEmpty(item.Tooltip) ? item.Name : item.Tooltip;
            if (item.ItemId > 0)
            {
                tooltipText = string.IsNullOrEmpty(tooltipText)
                    ? "Item ID: " + item.ItemId
                    : tooltipText + "\nItem ID: " + item.ItemId;
            }
            if (!string.IsNullOrEmpty(tooltipText))
            {
                border.ToolTip = new ToolTip
                {
                    Background = TooltipBg,
                    BorderBrush = TooltipBorder,
                    Foreground = TooltipText,
                    Content = new TextBlock
                    {
                        Text = tooltipText,
                        FontFamily = new FontFamily("Consolas"),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 340
                    }
                };
                ToolTipService.SetInitialShowDelay(border, 150);
                ToolTipService.SetShowDuration(border, 120000);
            }
            return border;
        }

        private static UIElement PanelHeader(string text)
        {
            return new Border
            {
                BorderBrush = PanelBorder,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(14, 11, 14, 11),
                Child = new TextBlock { Text = text, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.White }
            };
        }

        private static FrameworkElement MakePanel(UIElement content)
        {
            return new Border
            {
                Background = PanelBg,
                BorderBrush = PanelBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 0, 12),
                Child = content
            };
        }
    }
}
