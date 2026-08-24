namespace Befriender.UI.Configuration.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using System;

public class ConfigTab : ITab {
    private IConfigurationService configurationService;
    private readonly int[] availableIntervals = { 5, 15, 30 };

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
        ImGui.Text("Background Synchronization");
        ImGui.Separator();

        var currentInterval = config.SyncIntervalMinutes;
        var currentIndex = Array.IndexOf(this.availableIntervals, currentInterval);
        if (currentIndex == -1) {
            currentIndex = 1;
        }

        var previewValue = $"{this.availableIntervals[currentIndex]} minutes";

        if (ImGui.BeginCombo("Sync Interval", previewValue)) {
            for (int i = 0; i < this.availableIntervals.Length; i++) {
                bool isSelected = currentIndex == i;
                if (ImGui.Selectable($"{this.availableIntervals[i]} minutes", isSelected)) {
                    config.SyncIntervalMinutes = this.availableIntervals[i];
                    configChanged = true;
                }

                if (isSelected) {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        if (configChanged) {
            this.configurationService.Save();
        }
    }
}