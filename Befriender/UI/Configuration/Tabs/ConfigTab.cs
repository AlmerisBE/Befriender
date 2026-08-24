namespace Befriender.UI.Configuration.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;

public class ConfigTab : ITab {
    private IConfigurationService configurationService;

    public string Name => "Configuration";

    public ConfigTab(IConfigurationService configurationService) {
        this.configurationService = configurationService;
    }

    public void Draw() {
        var config = this.configurationService.GetConfig();
        bool configChanged = false;

        ImGui.Text("Automation Settings");
        ImGui.Separator();

        bool syncOnLogin = config.SyncOnLogin;
        if (ImGui.Checkbox("Sync on Login", ref syncOnLogin)) {
            config.SyncOnLogin = syncOnLogin;
            configChanged = true;
        }

        bool syncOnTerritory = config.SyncOnTerritoryChange;
        if (ImGui.Checkbox("Sync on Zone Change", ref syncOnTerritory)) {
            config.SyncOnTerritoryChange = syncOnTerritory;
            configChanged = true;
        }

        bool syncOnFriendList = config.SyncOnFriendListChange;
        if (ImGui.Checkbox("Sync on Friend Added/Removed", ref syncOnFriendList)) {
            config.SyncOnFriendListChange = syncOnFriendList;
            configChanged = true;
        }

        ImGui.Spacing();
        ImGui.Text("Background Synchronization (When Window is Open)");
        ImGui.Separator();

        int min = config.MinSyncIntervalMinutes;
        if (ImGui.SliderInt("Min Interval (min)", ref min, 5, 45)) {
            config.MinSyncIntervalMinutes = min;
            if (config.MaxSyncIntervalMinutes - min < 15) {
                config.MaxSyncIntervalMinutes = min + 15;
            }
            configChanged = true;
        }

        int max = config.MaxSyncIntervalMinutes;
        if (ImGui.SliderInt("Max Interval (min)", ref max, 20, 60)) {
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