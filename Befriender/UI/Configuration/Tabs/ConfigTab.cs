namespace Befriender.UI.Configuration.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;

public class ConfigTab : ITab {
    private IConfigurationService configurationService;
    private ILocalizationService loc;

    public string Name => this.loc.Translate("Tab_Config");

    public ConfigTab(IConfigurationService configurationService, ILocalizationService loc) {
        this.configurationService = configurationService;
        this.loc = loc;
    }

    public void Draw() {
        var config = this.configurationService.GetConfig();
        bool configChanged = false;

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

        if (configChanged) {
            this.configurationService.Save();
        }
    }
}