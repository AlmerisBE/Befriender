namespace Befriender.UI.Configuration.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

public class ConfigTab : ITab {
    private IConfigurationService configurationService;
    private ILocalizationService loc;
    private IThemeService themeService;

    public string Name => this.loc.Translate("Tab_Config");

    public ConfigTab(IConfigurationService configurationService, ILocalizationService loc, IThemeService themeService) {
        this.configurationService = configurationService;
        this.loc = loc;
        this.themeService = themeService;
    }

    public void Draw() {
        var config = this.configurationService.GetConfig();
        bool configChanged = false;

        ImGui.Text(this.loc.Translate("Config_ThemeSettings"));
        ImGui.Separator();

        var themes = this.themeService.GetAvailableThemes();
        var themeArray = themes.ToArray();
        int currentIndex = Math.Max(0, Array.IndexOf(themeArray, this.themeService.CurrentThemeName));

        if (ImGui.Combo(this.loc.Translate("Config_Theme"), ref currentIndex, themeArray, themeArray.Length)) {
            this.themeService.SetTheme(themeArray[currentIndex]);
        }

        ImGui.Spacing();
        ImGui.Text(this.loc.Translate("Config_ThemesDirectory"));

        string themeDir = this.themeService.ThemesDirectory;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100f);
        ImGui.InputText("##themeDir", ref themeDir, 1024, ImGuiInputTextFlags.ReadOnly);
        ImGui.SameLine();

        if (ImGui.Button(this.loc.Translate("Config_OpenDirectory"), new Vector2(90f, 0))) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = this.themeService.ThemesDirectory,
                    UseShellExecute = true
                });
            }
            catch {
                // Ignore if the OS fails to open the directory
            }
        }

        ImGui.Spacing();
        ImGui.Text(this.loc.Translate("Config_AutomationSettings"));
        ImGui.Separator();

        bool syncOnLogin = config.SyncOnLogin;
        if (ImGui.Checkbox(this.loc.Translate("Config_SyncOnLogin"), ref syncOnLogin)) {
            config.SyncOnLogin = syncOnLogin;
            configChanged = true;
        }

        bool syncOnTerritory = config.SyncOnTerritoryChange;
        if (ImGui.Checkbox(this.loc.Translate("Config_SyncOnZoneChange"), ref syncOnTerritory)) {
            config.SyncOnTerritoryChange = syncOnTerritory;
            configChanged = true;
        }

        bool syncOnFriendList = config.SyncOnFriendListChange;
        if (ImGui.Checkbox(this.loc.Translate("Config_SyncOnFriendListChange"), ref syncOnFriendList)) {
            config.SyncOnFriendListChange = syncOnFriendList;
            configChanged = true;
        }

        ImGui.Spacing();
        ImGui.Text(this.loc.Translate("Config_BackgroundSync"));
        ImGui.Separator();

        int min = config.MinSyncIntervalMinutes;
        if (ImGui.SliderInt(this.loc.Translate("Config_MinInterval"), ref min, 5, 45)) {
            config.MinSyncIntervalMinutes = min;
            if (config.MaxSyncIntervalMinutes - min < 15) {
                config.MaxSyncIntervalMinutes = min + 15;
            }

            configChanged = true;
        }

        int max = config.MaxSyncIntervalMinutes;
        if (ImGui.SliderInt(this.loc.Translate("Config_MaxInterval"), ref max, 20, 60)) {
            config.MaxSyncIntervalMinutes = max;
            if (max - config.MinSyncIntervalMinutes < 15) {
                config.MinSyncIntervalMinutes = max - 15;
            }

            configChanged = true;
        }

        ImGui.Spacing();
        ImGui.Text(this.loc.Translate("Config_HotkeySettings"));
        ImGui.Separator();

        bool ctrl = config.HotkeyCtrl;
        if (ImGui.Checkbox("Ctrl", ref ctrl)) { config.HotkeyCtrl = ctrl; configChanged = true; }
        ImGui.SameLine();

        bool shift = config.HotkeyShift;
        if (ImGui.Checkbox("Shift", ref shift)) { config.HotkeyShift = shift; configChanged = true; }
        ImGui.SameLine();

        bool alt = config.HotkeyAlt;
        if (ImGui.Checkbox("Alt", ref alt)) { config.HotkeyAlt = alt; configChanged = true; }

        var keys = Enum.GetValues<VirtualKey>();
        var keyNames = keys.Select(k => k.ToString()).ToArray();
        int currentKeyIndex = Array.IndexOf(keys, config.Hotkey);
        if (currentKeyIndex < 0) {
            currentKeyIndex = 0;
        }

        if (ImGui.Combo(this.loc.Translate("Config_Hotkey"), ref currentKeyIndex, keyNames, keyNames.Length)) {
            config.Hotkey = keys[currentKeyIndex];
            configChanged = true;
        }

        if (configChanged) {
            this.configurationService.Save();
        }
    }
}